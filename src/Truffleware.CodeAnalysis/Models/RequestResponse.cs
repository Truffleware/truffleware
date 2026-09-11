using Truffleware.CodeAnalysis.Utilities;

namespace Truffleware.CodeAnalysis.Models;

internal record RequestResponse(
    string RequestName,
    ValueArray<string> RequestNamespaces,
    string ResponseName,
    ValueArray<string> ResponseNamespaces,
    LightweightLocation Location);

internal class RequestResponseComparer : IEqualityComparer<RequestResponse>
{
    public bool Equals(RequestResponse? x, RequestResponse? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        // Skip location on purpose
        return x.RequestName == y.RequestName &&
               x.RequestNamespaces == y.RequestNamespaces &&
               x.ResponseName == y.ResponseName &&
               x.ResponseNamespaces == y.ResponseNamespaces;
    }

    public int GetHashCode(RequestResponse obj)
    {
        // Skip location on purpose
        return HashCode.Combine(
            obj.RequestName,
            obj.RequestNamespaces,
            obj.ResponseName,
            obj.ResponseNamespaces);
    }
}
