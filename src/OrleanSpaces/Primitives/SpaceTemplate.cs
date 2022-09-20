﻿using Orleans.Concurrency;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OrleanSpaces.Primitives;

/// <summary>
/// Represents a template (<i>or passive tuple</i>) in the tuple space paradigm.
/// </summary>
[Immutable]
public readonly struct SpaceTemplate : ITuple, IEquatable<SpaceTemplate>, IComparable<SpaceTemplate>
{
    private readonly object[] fields;

    public object this[int index] => fields[index];
    public int Length => fields.Length;

    /// <summary>
    /// Default constructor which always instantiates a <see cref="SpaceTemplate"/> with a single field of type <see cref="SpaceUnit"/>.
    /// </summary>
    public SpaceTemplate() : this(null)
    {
        
    }

    /// <summary>
    /// Main constructor which instantiates a <see cref="SpaceTemplate"/>, when all <paramref name="fields"/> are of valid type.
    /// </summary>
    /// <param name="fields">The fields of this template.</param>
    /// <remarks><i>Template fields can have types of: <see cref="SpaceUnit"/>, <see cref="Type"/>, <see cref="Type.IsPrimitive"/>, <see cref="Enum"/>, <see cref="string"/>, 
    /// <see cref="decimal"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="TimeSpan"/>, <see cref="Guid"/>.</i></remarks>
    /// <exception cref="ArgumentException"/>
    public SpaceTemplate([AllowNull] params object[] fields)
    {
        if (fields == null || fields.Length == 0)
        {
            this.fields = new object[1] { SpaceUnit.Null };
        }
        else
        {
            this.fields = new object[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                object obj = fields[i];
                if (obj is null)
                {
                    throw new ArgumentException($"The field at position = {i} can not be null.");
                }

                Type type = obj.GetType();

                if (!TypeChecker.IsSimpleType(type) && type != typeof(SpaceUnit) && obj is not Type)
                {
                    throw new ArgumentException($"The field at position = {i} is not a valid type.");
                }

                this.fields[i] = obj;
            }
        }
    }

    public bool Matches(SpaceTuple tuple)
    {
        if (tuple.Length != Length)
        {
            return false;
        }

        for (int i = 0; i < tuple.Length; i++)
        {
            if (this[i] is not SpaceUnit)
            {
                if (this[i] is Type templateType)
                {
                    if (!templateType.Equals(tuple[i].GetType()))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!this[i].Equals(tuple[i]))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public static implicit operator SpaceTemplate(SpaceTuple tuple)
    {
        object[] fields = new object[tuple.Length];

        for (int i = 0; i < tuple.Length; i++)
        {
            fields[i] = tuple[i];
        }

        return new(fields);
    }

    public static bool operator ==(SpaceTemplate left, SpaceTemplate right) => left.Equals(right);
    public static bool operator !=(SpaceTemplate left, SpaceTemplate right) => !(left == right);

    /// <summary>
    /// Determines whether the specified <see cref="object" /> is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is of type <see cref="SpaceTuple"/> and <see cref="Equals(SpaceTuple)"/> returns <see langword="true"/>; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SpaceTemplate template && Equals(template);
    /// <summary>
    /// Determines whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if all the fields of <see langword="this"/> match <paramref name="other"/> on the type, value and index; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SpaceTemplate other)
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

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>Whatever is the result of comparison between the Length's of <see langword="this"/> and <paramref name="other"/>.</returns>
    public int CompareTo(SpaceTemplate other) => Length.CompareTo(other.Length);

    public override int GetHashCode() => fields.GetHashCode();

    public override string ToString() => $"({string.Join(", ", fields)})";
}