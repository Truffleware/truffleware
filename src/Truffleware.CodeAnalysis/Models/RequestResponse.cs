using Truffleware.CodeAnalysis.Utilities;

namespace Truffleware.CodeAnalysis.Models;

internal record RequestResponse(
    string RequestName,
    ValueArray<string> RequestNamespaces,
    string ResponseName,
    ValueArray<string> ResponseNamespaces,
    LightweightLocation Location);
