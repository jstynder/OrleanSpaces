﻿using Orleans.Concurrency;
using System.Runtime.Intrinsics;

namespace OrleanSpaces.Tuples.Typed;

[Immutable]
public readonly struct DecimalTuple : IValueTuple<decimal, DecimalTuple>, ISpanFormattable
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>-79228162514264337593543950335</example>
    internal const int MaxFieldCharLength = 30;

    private readonly decimal[] fields;

    public ref readonly decimal this[int index] => ref fields[index];
    public int Length => fields.Length;

    public DecimalTuple() : this(Array.Empty<decimal>()) { }
    public DecimalTuple(params decimal[] fields) => this.fields = fields;

    public static bool operator ==(DecimalTuple left, DecimalTuple right) => left.Equals(right);
    public static bool operator !=(DecimalTuple left, DecimalTuple right) => !(left == right);

    public override bool Equals(object? obj) => obj is DecimalTuple tuple && Equals(tuple);

    public bool Equals(DecimalTuple other)
    {
        if (Vector256.IsHardwareAccelerated)
        {
            // We check if 256-bit vector operations are subject to hardware acceleration because a decimal is converted to its equivalent binary representation via 4 int's.
            // If the hardware supported only 128-bit vector operations it would result in [Vector<int>.Count = 4], which means 4 int comparission in parallel.
            // While true that it would be a single processor instruction, so is the direct comparission between 2 decimals, thereby we go this route only if there is 256-bit vector support
            // as this allows equality checking between 4 decimals (2 from 'this' and 2 from 'other', as opposed to 1 from 'this' and 1 from 'other').

            if (Length != other.Length)
            {
                return false;
            }

            if (Length == 1)
            {
                // If 'this' and 'other' are of tupleLength = 1, then there is no benefit to perform vector equality as the vectors will contain 4 int's.
                // Since this part of the code will only run if 256-bit vector operations are subject to hardware acceleration, it means that [4 / Vector<int>.Count] = [4 / 8] = [0].
                // This effectively means switching to sequential comparission between the 4 int's, so it makes sense to directly compare the 2 decimals.

                return this[0] == other[0];
            }

            int capacity = 8 * Length;  // 8x because each decimal will be decomposed into 4 ints, and we have 2 tuples to compare.
            return new Comparer(this, other).Execute(capacity);
        }

        return this.SequentialEquals(other);
    }

    public int CompareTo(DecimalTuple other) => Length.CompareTo(other.Length);

    public override int GetHashCode() => fields.GetHashCode();
    public override string ToString() => $"({string.Join(", ", fields)})";

    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => this.TryFormat(MaxFieldCharLength, destination, out charsWritten);

    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public ReadOnlySpan<decimal>.Enumerator GetEnumerator() => new ReadOnlySpan<decimal>(fields).GetEnumerator();

    internal readonly struct Comparer : IBufferConsumer<int>
    {
        private readonly DecimalTuple left;
        private readonly DecimalTuple right;

        public Comparer(DecimalTuple left, DecimalTuple right)
        {
            this.left = left;
            this.right = right;
        }

        public bool Consume(ref Span<int> buffer)
        {
            int tupleLength = left.Length;
            int bufferHalfLength = buffer.Length / 2;

            Span<int> leftSpan = buffer[..bufferHalfLength];
            Span<int> rightSpan = buffer[bufferHalfLength..];

            for (int i = 0; i < tupleLength; i++)
            {
                decimal.GetBits(left[i], leftSpan.Slice(i * 4, 4));
                decimal.GetBits(right[i], rightSpan.Slice(i * 4, 4));
            }

            return leftSpan.ParallelEquals(rightSpan);
        }
    }
}