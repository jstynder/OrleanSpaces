﻿using Newtonsoft.Json;
using OrleanSpaces.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace OrleanSpaces.Tuples.Typed;

[GenerateSerializer, Immutable]
public readonly struct ULongTuple : INumericTuple<ulong>, IEquatable<ULongTuple>
{
    [Id(0), JsonProperty] private readonly ulong[] fields;
    [JsonIgnore] public int Length => fields.Length;

    public ref readonly ulong this[int index] => ref fields[index];

    Span<ulong> INumericTuple<ulong>.Fields => fields.AsSpan();

    public ULongTuple() : this(Array.Empty<ulong>()) { }
    public ULongTuple(params ulong[] fields) => this.fields = fields;

    public static bool operator ==(ULongTuple left, ULongTuple right) => left.Equals(right);
    public static bool operator !=(ULongTuple left, ULongTuple right) => !(left == right);

    public override bool Equals(object? obj) => obj is ULongTuple tuple && Equals(tuple);
    public bool Equals(ULongTuple other) 
        => this.TryParallelEquals(other, out bool result) ? result : this.SequentialEquals(other);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => TupleHelpers.ToString(fields);

    static ISpaceTuple<ulong> ISpaceTuple<ulong>.Create(ulong[] fields) => new ULongTuple(fields);
    ISpaceTemplate<ulong> ISpaceTuple<ulong>.ToTemplate()
    {
        ref ulong?[] fields = ref TupleHelpers.CastAs<ulong[], ulong?[]>(in this.fields);
        return new ULongTemplate(fields);
    }

    public ReadOnlySpan<char> AsSpan() => this.AsSpan(Constants.MaxFieldCharLength_ULong);
    public ReadOnlySpan<ulong>.Enumerator GetEnumerator() => new ReadOnlySpan<ulong>(fields).GetEnumerator();
}

public readonly record struct ULongTemplate : ISpaceTemplate<ulong>
{
    private readonly ulong?[] fields;

    public ref readonly ulong? this[int index] => ref fields[index];
    public int Length => fields.Length;

    public ULongTemplate([AllowNull] params ulong?[] fields)
        => this.fields = fields == null || fields.Length == 0 ? new ulong?[1] { null } : fields;

    public bool Matches<TTuple>(TTuple tuple) where TTuple : ISpaceTuple<ulong>
        => TupleHelpers.Matches<ulong, ULongTuple>(this, tuple);

    public override string ToString() => TupleHelpers.ToString(fields);
    public ReadOnlySpan<ulong?>.Enumerator GetEnumerator() => new ReadOnlySpan<ulong?>(fields).GetEnumerator();
}
