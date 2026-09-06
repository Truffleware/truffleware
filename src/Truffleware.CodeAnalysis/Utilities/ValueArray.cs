using System.Collections;
using System.Collections.Immutable;

namespace Truffleware.CodeAnalysis.Utilities;

/// <summary>
/// Container that does equality checks based on actual values instead of reference equalities or anything similar.
/// </summary>
/// <remarks>
/// Source generator pipelines check values based on equality for caching purposes, which this is supposed to address.
/// </remarks>
/// <typeparam name="T">The type of values in the collection.</typeparam>
internal readonly struct ValueArray<T> : IEquatable<ValueArray<T>>, IReadOnlyList<T>
{
    private readonly ImmutableArray<T> _items;

    private ValueArray(ImmutableArray<T> items)
    {
        _items = items.IsDefault ? ImmutableArray<T>.Empty : items;
    }

    public static implicit operator ValueArray<T>(ImmutableArray<T> array)
        => new(array);

    public static implicit operator ImmutableArray<T>(ValueArray<T> array)
        => array._items;

    public bool Equals(ValueArray<T> other) =>
        _items.SequenceEqual(other._items);

    public override bool Equals(object? obj) =>
        obj is ValueArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var item in _items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    public T this[int index] => _items[index];

    public int Count => _items.IsDefault ? 0 : _items.Length;

    public static bool operator ==(ValueArray<T> left, ValueArray<T> right)
        => left.Equals(right);

    public static bool operator !=(ValueArray<T> left, ValueArray<T> right)
        => !left.Equals(right);

    public IEnumerator<T> GetEnumerator()
        => ((IEnumerable<T>)(_items.IsDefault ? ImmutableArray<T>.Empty : _items)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
