﻿using Newtonsoft.Json;
using OrleanSpaces.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace OrleanSpaces.Tuples.Specialized;

[GenerateSerializer, Immutable]
public readonly struct ShortTuple :
    IEquatable<ShortTuple>,
    INumericTuple<short>, 
    ISpaceFactory<short, ShortTuple>,
    ISpaceConvertible<short, ShortTemplate>
{
    [Id(0), JsonProperty] private readonly short[] fields;
    [JsonIgnore] public int Length => fields.Length;

    public ref readonly short this[int index] => ref fields[index];

    Span<short> INumericTuple<short>.Fields => fields.AsSpan();

    public ShortTuple() => fields = Array.Empty<short>();
    public ShortTuple([AllowNull] params short[] fields)
        => this.fields = fields is null ? Array.Empty<short>() : fields;

    public static bool operator ==(ShortTuple left, ShortTuple right) => left.Equals(right);
    public static bool operator !=(ShortTuple left, ShortTuple right) => !(left == right);

    public ShortTemplate ToTemplate()
    {
        int length = Length;
        short?[] fields = new short?[length];

        for (int i = 0; i < length; i++)
        {
            fields[i] = this[i];
        }

        return new ShortTemplate(fields);
    }

    public override bool Equals(object? obj) => obj is ShortTuple tuple && Equals(tuple);
    public bool Equals(ShortTuple other)
        => this.TryParallelEquals(other, out bool result) ? result : this.SequentialEquals(other);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => TupleHelpers.ToString(fields);

    static ShortTuple ISpaceFactory<short, ShortTuple>.Create(short[] fields) => new(fields);

    public ReadOnlySpan<char> AsSpan() => this.AsSpan(Constants.MaxFieldCharLength_Short);
    public ReadOnlySpan<short>.Enumerator GetEnumerator() => new ReadOnlySpan<short>(fields).GetEnumerator();
}

public readonly record struct ShortTemplate : ISpaceTemplate<short>, ISpaceMatchable<short, ShortTuple>
{
    private readonly short?[] fields;

    public ref readonly short? this[int index] => ref fields[index];
    public int Length => fields.Length;

    public ShortTemplate() => fields = Array.Empty<short?>();
    public ShortTemplate([AllowNull] params short?[] fields)
        => this.fields = fields is null ? Array.Empty<short?>() : fields;

    public bool Matches(ShortTuple tuple) => TupleHelpers.Matches<short, ShortTuple>(this, tuple);

    public override string ToString() => TupleHelpers.ToString(fields);
    public ReadOnlySpan<short?>.Enumerator GetEnumerator() => new ReadOnlySpan<short?>(fields).GetEnumerator();
}
