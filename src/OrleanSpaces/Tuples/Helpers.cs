﻿using OrleanSpaces.Tuples.Typed;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OrleanSpaces.Tuples;

internal static class Helpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSimpleType(this Type type) =>
       type.IsPrimitive ||
       type.IsEnum ||
       type == typeof(string) ||
       type == typeof(decimal) ||
       type == typeof(DateTime) ||
       type == typeof(DateTimeOffset) ||
       type == typeof(TimeSpan) ||
       type == typeof(Guid);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Execute<T>(this IBufferConsumer<T> consumer, int capacity)
       where T : unmanaged
    {
        if (capacity * Unsafe.SizeOf<T>() <= 1024) // It is good practice not to allocate more than 1 kilobyte of memory on the stack
        {
            Span<T> buffer = stackalloc T[capacity];
            return consumer.Consume(ref buffer);
        }
        else if (capacity <= 1048576)  // 1,048,576 is the maximum array length of ArrayPool.Shared
        {
            T[] pooledArray = ArrayPool<T>.Shared.Rent(capacity);
            Span<T> buffer = pooledArray.AsSpan();

            bool result = consumer.Consume(ref buffer);

            ArrayPool<T>.Shared.Return(pooledArray);

            return result;
        }
        else
        {
            T[] array = new T[capacity];
            Span<T> buffer = array.AsSpan();

            return consumer.Consume(ref buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParallelEquals<T, TSelf>(this INumericValueTuple<T, TSelf> left, INumericValueTuple<T, TSelf> right, out bool result)
        where T : unmanaged, INumber<T>
        where TSelf : INumericValueTuple<T, TSelf>
    {
        result = false;

        if (left.Length != right.Length)
        {
            return true;
        }

        if (!Vector.IsHardwareAccelerated)
        {
            return false;
        }

        result = ParallelEquals(left.Fields, right.Fields);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParallelEquals<TIn, TOut>(this NumericMarshaller<TIn, TOut> marshaller, out bool result)
        where TIn : struct
        where TOut : unmanaged, INumber<TOut>
    {
        result = false;

        if (marshaller.Left.Length != marshaller.Right.Length)
        {
            return true;
        }

        if (!Vector.IsHardwareAccelerated)
        {
            return false;
        }

        result = ParallelEquals(marshaller.Left, marshaller.Right);
        return true;
    }

    /// <remarks><i>Ensure the <see cref="NumericMarshaller{TIn, TOut}.Left"/> and <see cref="NumericMarshaller{TIn, TOut}.Right"/> are of equal capacity beforehand.</i></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParallelEquals<TIn, TOut>(this NumericMarshaller<TIn, TOut> marshaller)
        where TIn : struct
        where TOut : unmanaged, INumber<TOut>
        => ParallelEquals(marshaller.Left, marshaller.Right);

    /// <remarks><i>Ensure the <see cref="Span{T}.Length"/>(s) of <paramref name="left"/> and <paramref name="right"/> are of equal capacity beforehand.</i></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParallelEquals<T>(this Span<T> left, Span<T> right)
        where T : unmanaged, INumber<T>
    {
        int length = left.Length;
        if (length == 0)
        {
            return true;  // no elements, therefor 'left' & 'right' are equal.
        }

        if (length == 1)
        {
            return left[0] == right[0];  // avoiding overhead by doing a non-vectorized equality check, as its a single operation eitherway.
        }

        int vCount = Vector<T>.Count;
        int vLength = length / vCount;

        if (vLength == 0)
        {
            vLength = 1;
        }

        int i = 0;

        for (; i < vLength; i++)
        {
            Vector<T> vLeft = CastAsVector(left, i * vCount, vCount);
            Vector<T> vRight = CastAsVector(right, i * vCount, vCount);

            if (vLeft != vRight)
            {
                return false;
            }
        }

        i *= vCount;

        int remaining = length - i;
        if (remaining < 1)
        {
            return true;  // means [capacity % vCount = 0] therefor all elements have been compared (in parallel), and none were different (otherwise 'false' would have been returned) 
        }

        if (remaining == 1)
        {
            return left[i] == right[i];  // avoiding overhead by doing a non-vectorized equality check, as its a single operation eitherway.
        }

        Vector<T> _vLeft = CastAsVector(left, i, remaining);   // vector will have [i + vCount - remaining] elements set to default(TValue)
        Vector<T> _vRight = CastAsVector(right, i, remaining); // vector will have [i + vCount - remaining] elements set to default(TValue)

        return _vLeft == _vRight;

        static Vector<T> CastAsVector(Span<T> span, int start, int count)
        {
            int sLength = span.Length;
            if (sLength > count)
            {
                span = span.Slice(start, count);
            }

            int vLength = Vector<T>.Count;
            if (sLength == vLength)
            {
                // safe to create the vector directly from the span, as the lengths are equal (parent method will never supply a span that is [sLength > vLength])
                return CastAs<T, Vector<T>>(in span[0]);
            }

            // In cases where [sLength < vLength], if we try to create a vector from the span directly, it will result in a vector which has the first N items the same as the span,
            // but the rest will not be consistent for subsequent calls, even if a second span over a memory that contains the same values of 'TValue' as the first span did.
            // That is why we need to create a temporary span with the length equal to that of the vLength for the given type 'TValue', initialize all items to TValue.Zero a.k.a 'the default',
            // proceed to copy the contents of the original span into it. This is necessary because if we created the vector directly from the original span, and since [sLength < vLength],
            // the vector may contain garbage values in its remaining elements, which could cause incorrect results when compared with another vector which may contain other garbage values.

            Span<T> tempSpan = stackalloc T[vLength];
            tempSpan.Fill(T.Zero);
            span.CopyTo(tempSpan);
            
            return CastAs<T, Vector<T>>(in tempSpan[0]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SequentialEquals<T, TSelf>(this IObjectTuple<T, TSelf> left, IObjectTuple<T, TSelf> right)
        where T : notnull
        where TSelf : IObjectTuple<T, TSelf>
    {
        int length = left.Length;
        if (length != right.Length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (!left[i].Equals(right[i]))
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SequentialEquals<T, TSelf>(this IValueTuple<T, TSelf> left, IValueTuple<T, TSelf> right)
         where T : struct
         where TSelf : IValueTuple<T, TSelf>
    {
        int length = left.Length;
        if (length != right.Length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            ref readonly T iLeft = ref left[i];
            ref readonly T iRight = ref right[i];

            if (!iLeft.Equals(iRight))
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormat<T, TSelf>(this IValueTuple<T, TSelf> tuple, int maxFieldCharLength, Span<char> destination, out int charsWritten)
        where T : struct, ISpanFormattable
        where TSelf : IValueTuple<T, TSelf>
    {
        charsWritten = 0;

        if (tuple.Length == 0)
        {
            destination[charsWritten++] = '(';
            destination[charsWritten++] = ')';

            return true;
        }

        int totalLength = CalculateTotalLength(tuple.Length, maxFieldCharLength);
        ValueTupleFormatter<T, TSelf> formatter = new(tuple, maxFieldCharLength, ref charsWritten);

        // Depending on how large 'totalLength' is, the 'destination' which gets allocated from the runtime may not have enough capacity to accomodate the formatting
        // of the tupe. In this case we use the 'Execute<T>' method which will make sure to allocate the right amount of memory efficiently and execute the formatting logic.
        return destination.Length < totalLength ? formatter.Execute(totalLength) : formatter.Consume(ref destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateTotalLength(int tupleLength, int maxFieldCharLength)
    {
        int separatorsCount = 0;
        int destinationSpanLength = 0;

        if (tupleLength > 0)
        {
            separatorsCount = 2 * (tupleLength - 1);
            destinationSpanLength = maxFieldCharLength * tupleLength;
        }

        int totalLength = 2 + destinationSpanLength + separatorsCount;
        return totalLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TOut CastAs<TIn, TOut>(in TIn value) // 'value' is passed using 'in' to avoid defensive copying.
       where TIn : struct
       where TOut : struct
       => ref Unsafe.As<TIn, TOut>(ref Unsafe.AsRef(in value));
}
