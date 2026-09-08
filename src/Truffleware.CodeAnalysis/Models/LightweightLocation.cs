using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Truffleware.CodeAnalysis.Models;

/// <summary>
/// Lightweight version of Microsoft.CodeAnalysis.Location to avoid heavy objects in generator caches.
/// </summary>
internal record LightweightLocation(string? Filepath, TextSpan TextSpan, LinePositionSpan LinePositionSpan)
{
    public static implicit operator LightweightLocation(Location location)
    {
        return new LightweightLocation(
            location.SourceTree?.FilePath,
            location.SourceSpan,
            location.GetLineSpan().Span
        );
    }

    public static implicit operator Location(LightweightLocation location)
    {
        return Location.Create(location.Filepath ?? "", location.TextSpan, location.LinePositionSpan);
    }
}
