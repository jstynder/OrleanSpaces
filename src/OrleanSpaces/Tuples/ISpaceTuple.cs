﻿using System.Numerics;

namespace OrleanSpaces.Tuples;

/// <summary>
/// The base interface for any kind of space tuple.
/// </summary>
public interface ISpaceTuple 
{
    /// <summary>
    /// The length of this tuple.
    /// </summary>
    int Length { get; }
}

/// <summary>
/// The extended interface for any kind of specialized space tuple.
/// </summary>
/// <typeparam name="T">Any of the supported non-reference types.</typeparam>
public interface ISpaceTuple<T> : ISpaceTuple
    where T : unmanaged
{
    /// <summary>
    /// Returns a readonly reference of the field specified by <paramref name="index"/>.
    /// </summary>
    ref readonly T this[int index] { get; }

    /// <summary>
    /// Returns a readonly span of characters which represent this tuple.
    /// </summary>
    ReadOnlySpan<char> AsSpan();

    /// <summary>
    /// Returns an enumerator to enumerate over the fields of this tuple.
    /// </summary>
    ReadOnlySpan<T>.Enumerator GetEnumerator();
}

internal interface INumericTuple<T> : ISpaceTuple<T>
    where T : unmanaged, INumber<T>
{
    Span<T> Fields { get; }
}

internal interface ISpaceConvertible<T, TTemplate>
    where T : unmanaged
    where TTemplate : ISpaceTemplate<T>
{
    TTemplate ToTemplate();
}

internal interface ISpaceFactory<T, TTuple>
    where T : unmanaged
    where TTuple : ISpaceTuple<T>
{
    static abstract TTuple Create(T[] fields);
}