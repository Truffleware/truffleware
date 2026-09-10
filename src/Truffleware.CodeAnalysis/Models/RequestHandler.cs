using Microsoft.CodeAnalysis;

using Truffleware.CodeAnalysis.Utilities;

namespace Truffleware.CodeAnalysis.Models;

internal record RequestHandler(
    string ClassName,
    string ClassNamespace,
    Accessibility Accessibility,
    ValueArray<RequestResponse> RequestResponses);
