﻿using Newtonsoft.Json;
using OrleanSpaces.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace OrleanSpaces.Tuples.Specialized;

[GenerateSerializer, Immutable]
public readonly struct ULongTuple :
    IEquatable<ULongTuple>,
    INumericTuple<ulong>, 
    ISpaceFactory<ulong, ULongTuple>,
    ISpaceConvertible<ulong, ULongTemplate>
{
    [Id(0), JsonProperty] private readonly ulong[] fields;
    [JsonIgnore] public int Length => fields.Length;

    public ref readonly ulong this[int index] => ref fields[index];

    Span<ulong> INumericTuple<ulong>.Fields => fields.AsSpan();

    public ULongTuple() => fields = Array.Empty<ulong>();
    public ULongTuple([AllowNull] params ulong[] fields)
        => this.fields = fields is null ? Array.Empty<ulong>() : fields;

    public static bool operator ==(ULongTuple left, ULongTuple right) => left.Equals(right);
    public static bool operator !=(ULongTuple left, ULongTuple right) => !(left == right);

    public ULongTemplate ToTemplate()
    {
        int length = Length;
        ulong?[] fields = new ulong?[length];

        for (int i = 0; i < length; i++)
        {
            fields[i] = this[i];
        }

        return new ULongTemplate(fields);
    }

    public override bool Equals(object? obj) => obj is ULongTuple tuple && Equals(tuple);
    public bool Equals(ULongTuple other)
        => this.TryParallelEquals(other, out bool result) ? result : this.SequentialEquals(other);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => TupleHelpers.ToString(fields);

    static ULongTuple ISpaceFactory<ulong, ULongTuple>.Create(ulong[] fields) => new(fields);

    public ReadOnlySpan<char> AsSpan() => this.AsSpan(Constants.MaxFieldCharLength_ULong);
    public ReadOnlySpan<ulong>.Enumerator GetEnumerator() => new ReadOnlySpan<ulong>(fields).GetEnumerator();
}

public readonly record struct ULongTemplate : ISpaceTemplate<ulong>, ISpaceMatchable<ulong, ULongTuple>
{
    private readonly ulong?[] fields;

    public ref readonly ulong? this[int index] => ref fields[index];
    public int Length => fields.Length;

    public ULongTemplate() => fields = Array.Empty<ulong?>();
    public ULongTemplate([AllowNull] params ulong?[] fields)
        => this.fields = fields is null ? Array.Empty<ulong?>() : fields;

    public bool Matches(ULongTuple tuple) => TupleHelpers.Matches<ulong, ULongTuple>(this, tuple);

    public override string ToString() => TupleHelpers.ToString(fields);
    public ReadOnlySpan<ulong?>.Enumerator GetEnumerator() => new ReadOnlySpan<ulong?>(fields).GetEnumerator();
}
