﻿using System.Runtime.CompilerServices;

namespace OrleanSpaces.Primitives;

[Serializable]
public readonly struct SpaceTuple : ISpaceTuple, IEquatable<SpaceTuple>
{
    private readonly object[] fields;

    public ref readonly object this[int index] => ref fields[index];
    public int Length => fields?.Length ?? 0;

    public bool IsEmpty => fields == null || fields.Length == 0;

    public ReadOnlySpan<object> AsSpan() => new(fields);

    public SpaceTuple() : this(new object[0]) { }

    private SpaceTuple(params object[] fields)
    {
        if (fields.Any(x => x is SpaceUnit))
        {
            throw new ArgumentException($"'{nameof(SpaceUnit.Null)}' is not a valid '{nameof(SpaceTuple)}' field");
        }

        this.fields = fields ?? Array.Empty<object>();
    }

    public static SpaceTuple Create(ValueType value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is ITuple tuple)
        {
            var fields = new object[tuple.Length];
            for (int i = 0; i < tuple.Length; i++)
            {
                if (tuple[i] is not ValueType && 
                    tuple[i] is not string)
                {
                    throw new ArgumentException($"Reference types are not valid '{nameof(SpaceTuple)}' fields");
                }

                fields[i] = tuple[i];
            }

            return new(fields);
        }

        return new(value);
    }

    public static SpaceTuple Create(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value));
        }

        return new(value);
    }

    public static bool operator ==(SpaceTuple first, SpaceTuple second) => first.Equals(second);
    public static bool operator !=(SpaceTuple first, SpaceTuple second) => !(first == second);

    public override bool Equals(object obj) =>
        obj is SpaceTuple tuple && Equals(tuple);

    public bool Equals(SpaceTuple other)
    {
        if (Length != other.Length)
        {
            return false;
        }

        for (int i = 0; i < Length; i++)
        {
            if (!this[i].Equals(other[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode() => HashCode.Combine(fields, Length);

    public override string ToString() => $"<{string.Join(", ", fields)}>";
}