using Microsoft.CodeAnalysis;

namespace Truffleware.CodeAnalysis.Diagnostics;

internal static class DiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor Tw0001 = new(
        id: "TW0001",
        title: "Multiple declarations",
        messageFormat: "A handler for '{0}' was already declared by '{1}'",
        category: DiagnosticCategories.Usage,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each request-response pair may only have a single handler.",
        helpLinkUri: "TODO"
    );
}
