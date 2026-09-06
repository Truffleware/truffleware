using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
namespace Truffleware.CodeAnalysis.Utilities;

public static class TypeUtils
{
    public static readonly SymbolDisplayFormat NamespaceWithoutGlobals = SymbolDisplayFormat.FullyQualifiedFormat
        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    /// <summary>
    /// Return all namespaces referenced by a generic type.
    /// </summary>
    public static ImmutableArray<string> GetAllNamespaces(ITypeSymbol type, CancellationToken cancellationToken)
    {
        var namespaces = GetAllNamespacesCore(type, cancellationToken)
            .Distinct()
            .OrderBy(ns => ns);

        return [..namespaces];

        static IEnumerable<string> GetAllNamespacesCore(ITypeSymbol type, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!type.ContainingNamespace.IsGlobalNamespace)
            {
                yield return type.ContainingNamespace.ToDisplayString(NamespaceWithoutGlobals);
            }

            if (type is not INamedTypeSymbol { IsGenericType: true } generic)
            {
                yield break;
            }

            foreach (var ns in generic.TypeArguments.SelectMany(t => GetAllNamespacesCore(t, cancellationToken)))
            {
                yield return ns;
            }
        }
    }
}
