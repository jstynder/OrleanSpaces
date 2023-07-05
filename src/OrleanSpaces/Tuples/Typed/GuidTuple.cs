﻿using Newtonsoft.Json;
using OrleanSpaces.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics;

namespace OrleanSpaces.Tuples.Typed;

[GenerateSerializer, Immutable]
public readonly struct GuidTuple : ISpaceTuple<Guid>, IEquatable<GuidTuple>
{
    [Id(0), JsonProperty] private readonly Guid[] fields;
    [JsonIgnore] public int Length => fields.Length;

    public ref readonly Guid this[int index] => ref fields[index];
    
    public GuidTuple() : this(Array.Empty<Guid>()) { }
    public GuidTuple(params Guid[] fields) => this.fields = fields;

    public static bool operator ==(GuidTuple left, GuidTuple right) => left.Equals(right);
    public static bool operator !=(GuidTuple left, GuidTuple right) => !(left == right);

    public static explicit operator GuidTemplate(GuidTuple tuple)
    {
        ref Guid?[] fields = ref TupleHelpers.CastAs<Guid[], Guid?[]>(in tuple.fields);
        return new GuidTemplate(fields);
    }

    public override bool Equals(object? obj) => obj is GuidTuple tuple && Equals(tuple);

    public bool Equals(GuidTuple other)
    {
        if (Length != other.Length)
        {
            return false;
        }

        if (!Vector128.IsHardwareAccelerated)
        {
            return this.SequentialEquals(other);
        }

        for (int i = 0; i < Length; i++)
        {
            // We are transforming the managed pointer(s) of type 'Guid' (obtained after re-interpreting the readonly reference(s) 'fields[i]' and 'other.fields[i]' to new mutable reference(s))
            // to new managed pointer(s) of type 'Vector128<byte>' and comparing them.

            ref Vector128<byte> vLeft = ref TupleHelpers.CastAs<Guid, Vector128<byte>>(in fields[i]);
            ref Vector128<byte> vRight = ref TupleHelpers.CastAs<Guid, Vector128<byte>>(in other.fields[i]);

            if (vLeft != vRight)
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => TupleHelpers.ToString(fields);

    ISpaceTemplate<Guid> ISpaceTuple<Guid>.ToTemplate() => (GuidTemplate)this;
    static ISpaceTuple<Guid> ISpaceTuple<Guid>.Create(Guid[] fields) => new GuidTuple(fields);

    public ReadOnlySpan<char> AsSpan() => this.AsSpan(Constants.MaxFieldCharLength_Guid);
    public ReadOnlySpan<Guid>.Enumerator GetEnumerator() => new ReadOnlySpan<Guid>(fields).GetEnumerator();
}

public readonly record struct GuidTemplate : ISpaceTemplate<Guid>
{
    private readonly Guid?[] fields;

    public ref readonly Guid? this[int index] => ref fields[index];
    public int Length => fields.Length;

    public GuidTemplate([AllowNull] params Guid?[] fields)
        => this.fields = fields == null || fields.Length == 0 ? new Guid?[1] { null } : fields;

    public bool Matches<TTuple>(TTuple tuple) where TTuple : ISpaceTuple<Guid>
        => TupleHelpers.Matches<Guid, GuidTuple>(this, tuple);

    public override string ToString() => TupleHelpers.ToString(fields);
    public ReadOnlySpan<Guid?>.Enumerator GetEnumerator() => new ReadOnlySpan<Guid?>(fields).GetEnumerator();
}
