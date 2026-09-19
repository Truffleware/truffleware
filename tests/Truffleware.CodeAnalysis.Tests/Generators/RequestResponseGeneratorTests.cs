using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;

using Truffleware.Abstractions.Messaging;
using Truffleware.CodeAnalysis.Diagnostics;
using Truffleware.CodeAnalysis.Generators;

namespace Truffleware.CodeAnalysis.Tests.Generators;

[TestClass]
[UsesVerify]
public sealed partial class RequestResponseGeneratorTests
{
    private const string AssemblyName = "Truffleware.RequestResponseGenerator.Tests";
    private const string RequestResponseInputFile = "RequestResponseGeneratorInput.cs";
    private const string DuplicateHandlersInputFile = "RequestResponseGeneratorDuplicateHandlersInput.cs";

    [TestMethod]
    public Task GeneratesHandlerSenderAndServiceRegistrations()
    {
        var driver = RunGeneratorFromInputFile(RequestResponseInputFile, out var outputCompilation);

        AssertNoDiagnostics(outputCompilation);

        return Verify(driver);
    }

    [TestMethod]
    public void ReportsDiagnosticForDuplicateRequestResponseHandlers()
    {
        var driver = RunGeneratorFromInputFile(DuplicateHandlersInputFile, out _);

        var diagnostics = driver.GetRunResult()
            .Diagnostics
            .Where(diagnostic => diagnostic.Id == DiagnosticDescriptors.Tw0001.Id)
            .ToArray();

        var diagnostic = Assert.ContainsSingle(diagnostics);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.AreEqual(
            "A handler for 'RequestHandlerAttribute<DuplicatePing, DuplicatePong>' was already declared by 'FirstDuplicatePingHandler'",
            diagnostic.GetMessage());
    }

    private static GeneratorDriver RunGeneratorFromInputFile(string fileName, out Compilation outputCompilation)
    {
        var sourcePath = Path.Combine(GetInputsDirectory(), fileName);
        var sourceText = SourceText.From(File.ReadAllText(sourcePath));
        var compilation = CSharpCompilation.Create(
            assemblyName: AssemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(sourceText, path: sourcePath)],
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new RequestResponseGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out _);

        return driver;
    }

    private static void AssertNoDiagnostics(Compilation compilation)
    {
        AssertNoDiagnostics(compilation, DiagnosticSeverity.Error);
        AssertNoDiagnostics(compilation, DiagnosticSeverity.Warning);
        AssertNoDiagnostics(compilation, DiagnosticSeverity.Info);
        // Skip hidden on purpose
    }

    private static void AssertNoDiagnostics(Compilation compilation, DiagnosticSeverity severity)
    {
        var diagnostics = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == severity);

        Assert.IsNull(diagnostics.FirstOrDefault());
    }

    private static string GetInputsDirectory([CallerFilePath] string thisFilePath = "")
    {
        var relative = Path.Combine(Path.GetDirectoryName(thisFilePath)!, "..", "Inputs");
        var absolute = Path.GetFullPath(relative);

        return absolute;
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var assemblyLocations = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(static assembly =>
                !assembly.IsDynamic &&
                !string.IsNullOrEmpty(assembly.Location) &&
                assembly != typeof(RequestResponseGeneratorTests).Assembly)
            .Select(static assembly => assembly.Location)
            .Append(typeof(RequestHandlerAttribute<,>).Assembly.Location)
            .Append(typeof(IServiceCollection).Assembly.Location)
            .Distinct()
            .ToArray();

        return assemblyLocations.Select(static path => MetadataReference.CreateFromFile(path));
    }
}
