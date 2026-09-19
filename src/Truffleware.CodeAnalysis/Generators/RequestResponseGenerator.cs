using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Truffleware.CodeAnalysis.Diagnostics;
using Truffleware.CodeAnalysis.Extensions;
using Truffleware.CodeAnalysis.Utilities;
using Truffleware.CodeAnalysis.Models;

namespace Truffleware.CodeAnalysis.Generators;

/// <summary>
/// Auto-generates dependency injection links for one-to-one request-response messaging pairs.
/// </summary>
/// <remarks>
/// The intended purpose of this mechanism is to statically decouple two systems that need to exchange messages with
/// each other, but where we don't want them to directly know about each other's existence. This methodology further
/// allows for pipelining the requests and middleware to be created to pre- or post-process the messages.
/// </remarks>
[Generator(LanguageNames.CSharp)]
internal class RequestResponseGenerator : IIncrementalGenerator
{
    private const string AbstractionsNamespace = "Truffleware.Abstractions.Messaging";
    private const string RequestHandlerAttributeFullName = $"{AbstractionsNamespace}.RequestHandlerAttribute`2";

    private static readonly RequestResponseComparer _requestResponseComparer = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var handlerPipeline = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: RequestHandlerAttributeFullName,
                predicate: static (n, _) => n is ClassDeclarationSyntax,
                transform: TransformHandler)
            .Where(handler => handler is not null)
            .Select((handler, _) => handler!);
        context.RegisterImplementationSourceOutput(handlerPipeline, CreateHandler);

        var handlersPipeline = handlerPipeline.Collect();
        context.RegisterImplementationSourceOutput(handlersPipeline, CreateSender);

        context.RegisterImplementationSourceOutput(handlersPipeline, CreateServiceExtensionFile);
    }


    private static RequestHandler? TransformHandler(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ctx.TargetSymbol is not INamedTypeSymbol namedSymbol)
        {
            return null;
        }

        var className = namedSymbol.OriginalDefinition.Name;
        var classNamespace = namedSymbol.ContainingNamespace.ToDisplayString(TypeUtils.NamespaceWithoutGlobals);

        ct.ThrowIfCancellationRequested();

        var requestResponses = ctx.Attributes
            // TODO: Would there ever be multiple here, or would they be processed separately?
            .Select(a =>
            {
                ct.ThrowIfCancellationRequested();

                var requestType = a.AttributeClass?.TypeArguments.First()
                    ?? throw new InvalidOperationException("Could not resolve attribute type argument.");
                var responseType = a.AttributeClass?.TypeArguments.Skip(1).First()
                    ?? throw new InvalidOperationException("Could not resolve attribute type argument.");

                var requestName = requestType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var responseName = responseType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                ct.ThrowIfCancellationRequested();

                var requestNamespaces = TypeUtils.GetAllNamespaces(requestType, ct);
                var responseNamespaces = TypeUtils.GetAllNamespaces(responseType, ct);

                var location = a.ApplicationSyntaxReference?
                    .GetSyntax()
                    .GetLocation()
                    ?? throw new InvalidOperationException("Could not resolve attribute location.");

                return new RequestResponse(requestName, requestNamespaces, responseName, responseNamespaces, location);
            })
            .ToImmutableArray();

        return new RequestHandler(className, classNamespace, namedSymbol.DeclaredAccessibility, requestResponses);
    }

    private static void CreateSender(SourceProductionContext ctx, ImmutableArray<RequestHandler> handlers)
    {
        var requestResponseDuplicates = handlers
            .SelectMany(h => h.RequestResponses)
            .GroupBy(r => r, _requestResponseComparer)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key with { Location = Location.None })
            .ToList();
        var processed = new Dictionary<RequestResponse, RequestHandler>();
        var handlersSorted = handlers
            .OrderBy(h => h.ClassName);

        foreach (var handler in handlersSorted)
        {
            foreach (var requestResponse in handler.RequestResponses)
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (processed.TryGetValue(requestResponse with { Location = Location.None }, out var existing))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.Tw0001,
                        requestResponse.Location,
                        [
                            $"RequestHandlerAttribute<{requestResponse.RequestName}, {requestResponse.ResponseName}>",
                            existing.ClassName,
                        ]
                    );
                    ctx.ReportDiagnostic(diagnostic);

                    continue;
                }

                processed[requestResponse with { Location = Location.None }] = handler;

                List<string> usingSystemNamespaceNames = ["System.Threading.Tasks"];

                List<string> usingThirdPartyNamespaceNames =
                [
                    GeneratorConstants.NamespaceName,
                    AbstractionsNamespace,
                ];

                List<string> usingRequestResponseNamespaceNames =
                [
                    .. requestResponse.RequestNamespaces,
                    .. requestResponse.ResponseNamespaces,
                    handler.ClassNamespace,
                ];

                List<string> usingLines =
                [
                    ..FormatNamespaceNames(usingSystemNamespaceNames),
                    "",
                    ..FormatNamespaceNames(usingThirdPartyNamespaceNames),
                    "",
                    ..FormatNamespaceNames(usingRequestResponseNamespaceNames),
                ];
                var senderClassName = GetSenderClassName(requestResponse);

                string code = $$"""
                                // <auto-generated />
                                #nullable enable

                                {{string.Join("\n", usingLines)}}

                                namespace {{GeneratorConstants.NamespaceName}};

                                {{handler.Accessibility.ToKeyword()}} sealed class {{senderClassName}}(IRequestHandler<{{requestResponse.RequestName}}, {{requestResponse.ResponseName}}> handler)
                                    : IRequestSender<{{requestResponse.RequestName}}, {{requestResponse.ResponseName}}>
                                {
                                    public async Task<{{requestResponse.ResponseName}}> SendAsync({{requestResponse.RequestName}} request)
                                    {
                                        return await handler.HandleAsync(request);
                                    }
                                }
                                """;

                var filename = FilenameUtils.ToSafeShort($"{senderClassName}.g.cs");
                ctx.AddSource(filename, code);
            }
        }
    }

    private static void CreateHandler(SourceProductionContext ctx, RequestHandler requestHandler)
    {
        ctx.CancellationToken.ThrowIfCancellationRequested();

        List<string> usingLinesAll =
        [
            GeneratorConstants.NamespaceName,
            AbstractionsNamespace,
            requestHandler.ClassNamespace,
            ..requestHandler.RequestResponses.SelectMany(r => r.RequestNamespaces),
            ..requestHandler.RequestResponses.SelectMany(r => r.ResponseNamespaces),
        ];
        var usingLines = usingLinesAll
            .Where(ns => !string.IsNullOrWhiteSpace(ns) && ns != requestHandler.ClassNamespace)
            .Distinct()
            .OrderBy(ns => ns)
            .Select(ns => $"using global::{ns};");
        var namespaceLine = string.IsNullOrEmpty(requestHandler.ClassNamespace)
            ? "// No namespace in source decorated with our marker attribute"
            : $"namespace {requestHandler.ClassNamespace};";

        var interfaceLines = requestHandler.RequestResponses
            .Select(r => $"IRequestHandler<{r.RequestName}, {r.ResponseName}>");

        string code = $$"""
        // <auto-generated />
        #nullable enable

        {{string.Join("\n", usingLines)}}

        {{ namespaceLine }}

        {{requestHandler.Accessibility.ToKeyword()}} partial class {{requestHandler.ClassName}} :
            {{string.Join(",\n    ", interfaceLines)}}
        {
        }
        """;

        var filename = $"{FilenameUtils.ToSafeShort(requestHandler.ClassName)}.g.cs";
        ctx.AddSource(filename, code);
    }

    private static void CreateServiceExtensionFile(SourceProductionContext ctx, ImmutableArray<RequestHandler> handlers)
    {
        ctx.CancellationToken.ThrowIfCancellationRequested();

        List<string> usingLinesAll = [
            AbstractionsNamespace,
            ..handlers.Select(h => h.ClassNamespace),
            ..handlers.SelectMany(h => h.RequestResponses.SelectMany(r => r.RequestNamespaces)),
            ..handlers.SelectMany(h => h.RequestResponses.SelectMany(r => r.ResponseNamespaces)),
        ];
        var usingLines = usingLinesAll
            .Where(ns => !string.IsNullOrWhiteSpace(ns) && ns != GeneratorConstants.NamespaceName)
            .Distinct()
            .OrderBy(ns => ns)
            .Select(ns => $"using global::{ns};");

        var senderLines = handlers
            .SelectMany(h => h.RequestResponses)
            .Distinct()
            .Select(r => $"services.AddTransient<IRequestSender<{r.RequestName}, {r.ResponseName}>, {GetSenderClassName(r)}>();")
            .OrderBy(l => l);

        var handlerLines = handlers
            .SelectMany(h => h.RequestResponses, (handler, requestResponse) => (handler, requestResponse))
            .Select(x => $"services.AddTransient<IRequestHandler<{x.requestResponse.RequestName}, {x.requestResponse.ResponseName}>, {x.handler.ClassName}>();")
            .OrderBy(l => l);

        string code = $$"""
                         // <auto-generated />
                         #nullable enable

                         using global::Microsoft.Extensions.DependencyInjection;

                         {{string.Join("\n", usingLines)}}

                         namespace {{GeneratorConstants.NamespaceName}};

                         internal static partial class ServiceCollectionExtensions
                         {
                             public static IServiceCollection AddSenders(this IServiceCollection services)
                             {
                                 {{string.Join("\n        ", senderLines)}}

                                 return services;
                             }

                             public static IServiceCollection AddHandlers(this IServiceCollection services)
                             {
                                 // TODO: Warning if there's multiple handlers for the same request-response pair?
                                 {{string.Join("\n        ", handlerLines)}}

                                 return services;
                             }
                         }
                         """;

         ctx.AddSource("ServiceExtensions.g.cs", code);
    }

    private static List<string> FormatNamespaceNames(IEnumerable<string> namespaceNames)
        =>
        [
            .. namespaceNames
                .Distinct()
                .OrderBy(ns => ns)
                .Select(ns => $"using global::{ns};")
        ];

    private static string GetSenderClassName(RequestResponse requestResponse)
    {
        string rawName = $"Sender{requestResponse.RequestName}To{requestResponse.ResponseName}";
        string safeName = ToIdentifier(rawName);

        if (safeName.Length <= 80)
        {
            return safeName;
        }

        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(rawName));
        string hashString = BitConverter
            .ToString(hashBytes)
            .Replace("-", "")
            .Substring(0, 8);
        return $"{safeName.Substring(0, 71)}_{hashString}";
    }

    private static string ToIdentifier(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (char c in value)
        {
            builder.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
        }

        if (builder.Length == 0)
        {
            return "GeneratedType";
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }
}
