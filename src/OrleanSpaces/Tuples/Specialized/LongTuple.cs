﻿using Newtonsoft.Json;
using OrleanSpaces.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace OrleanSpaces.Tuples.Specialized;

/// <summary>
/// Represents a tuple which has <see cref="long"/> field types only.
/// </summary>
[GenerateSerializer, Immutable]
public readonly record struct LongTuple :
    IEquatable<LongTuple>,
    INumericTuple<long>, 
    ISpaceFactory<long, LongTuple>,
    ISpaceConvertible<long, LongTemplate>
{
    [Id(0), JsonProperty] private readonly long[] fields;
    [JsonIgnore] public int Length => fields?.Length ?? 0;

    public ref readonly long this[int index] => ref fields[index];

    Span<long> INumericTuple<long>.Fields => fields.AsSpan();

    /// <summary>
    /// Default constructor which instantiates an empty tuple. 
    /// </summary>
    public LongTuple() => fields = Array.Empty<long>();

    /// <summary>
    /// Main constructor which instantiates a non-empty tuple, when at least one field is supplied, otherwise an empty tuple is instantiated.
    /// </summary>
    /// <param name="fields">The elements of this tuple.</param>
    public LongTuple([AllowNull] params long[] fields)
        => this.fields = fields is null ? Array.Empty<long>() : fields;

    /// <summary>
    /// Returns a <see cref="LongTemplate"/> with the same fields as <see langword="this"/>.
    /// </summary>
    public LongTemplate ToTemplate()
    {
        int length = Length;
        long?[] fields = new long?[length];

        for (int i = 0; i < length; i++)
        {
            fields[i] = this[i];
        }

        return new LongTemplate(fields);
    }

    public bool Equals(LongTuple other)
        => this.TryParallelEquals(other, out bool result) ? result : this.SequentialEquals(other);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => SpaceHelpers.ToString(fields);

    static LongTuple ISpaceFactory<long, LongTuple>.Create(long[] fields) => new(fields);

    public ReadOnlySpan<char> AsSpan() => this.AsSpan(Constants.MaxFieldCharLength_Long);
    public ReadOnlySpan<long>.Enumerator GetEnumerator() => new ReadOnlySpan<long>(fields).GetEnumerator();
}

/// <summary>
/// Represents a template which has <see cref="long"/> field types only.
/// </summary>
public readonly record struct LongTemplate : 
    IEquatable<LongTemplate>,
    ISpaceTemplate<long>, 
    ISpaceMatchable<long, LongTuple>
{
    private readonly long?[] fields;

    public ref readonly long? this[int index] => ref fields[index];
    public int Length => fields?.Length ?? 0;

    /// <summary>
    /// Default constructor which instantiates an empty template. 
    /// </summary>
    public LongTemplate() => fields = Array.Empty<long?>();

    /// <summary>
    /// Main constructor which instantiates a non-empty template, when at least one field is supplied, otherwise an empty template is instantiated.
    /// </summary>
    /// <param name="fields">The elements of this template.</param>
    public LongTemplate([AllowNull] params long?[] fields)
        => this.fields = fields is null ? Array.Empty<long?>() : fields;

    /// <summary>
    /// Determines whether <see langword="this"/> matches the specified <paramref name="tuple"/>.
    /// </summary>
    /// <param name="tuple">A tuple to be matched by <see langword="this"/>.</param>
    /// <returns><see langword="true"/>, if <see langword="this"/> and <paramref name="tuple"/> share the same number of fields, and all of them match on the index and value 
    /// (<i>except when any field of <see langword="this"/> is of type <see langword="null"/></i>); otherwise, <see langword="false"/>.</returns>
    public bool Matches(LongTuple tuple) => this.Matches<long, LongTuple>(tuple);
    public bool Equals(LongTemplate other) => this.SequentialEquals(other);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => SpaceHelpers.ToString(fields);

    public ReadOnlySpan<long?>.Enumerator GetEnumerator() => new ReadOnlySpan<long?>(fields).GetEnumerator();
}
