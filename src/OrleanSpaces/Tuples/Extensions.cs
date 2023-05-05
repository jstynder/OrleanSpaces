﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace OrleanSpaces.Tuples;

internal static class Extensions
{
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

    /// <remarks><i>Ensure the <see cref="NumericMarshaller{TIn, TOut}.Left"/> and <see cref="NumericMarshaller{TIn, TOut}.Right"/> are of equal slots beforehand.</i></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParallelEquals<TIn, TOut>(this NumericMarshaller<TIn, TOut> marshaller)
        where TIn : struct
        where TOut : unmanaged, INumber<TOut>
        => ParallelEquals(marshaller.Left, marshaller.Right);

    /// <remarks><i>Ensure the <see cref="Span{T}.Length"/>(s) of <paramref name="left"/> and <paramref name="right"/> are of equal slots beforehand.</i></remarks>
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
            return true;  // means [slots % vCount = 0] therefor all elements have been compared (in parallel), and none were different (otherwise 'false' would have been returned) 
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
        destination.Clear();  // we don't know if the memory represented by the span might contain garbage values so we clear it.
        charsWritten = 0;

        int tupleLength = tuple.Length;
        int totalLength = CalculateTotalLength(tupleLength, maxFieldCharLength);

        if (tupleLength == 0)
        {
            destination[charsWritten++] = '(';
            destination[charsWritten++] = ')';

            return true;
        }

        // Its safe to allocate memory on the stack because the maxFieldCharLength is a constant on all tuples,
        // and has a finite value: [2 bytes (since 'char') * maxFieldCharLength < 1024 bytes]
        Span<char> fieldSpan = stackalloc char[maxFieldCharLength];

        if (tupleLength == 1)
        {
            destination[charsWritten++] = '(';

            if (!TryFormatField(in tuple[0], destination, fieldSpan, ref charsWritten))
            {
                return false;
            }

            destination[charsWritten++] = ')';

            return true;
        }

        destination[charsWritten++] = '(';

        for (int i = 0; i < tupleLength; i++)
        {
            if (i > 0)
            {
                destination[charsWritten++] = ',';
                destination[charsWritten++] = ' ';

                fieldSpan.Clear();
            }

            if (!TryFormatField(in tuple[i], destination, fieldSpan, ref charsWritten))
            {
                return false;
            }
        }

        destination[charsWritten++] = ')';

        return true;

        static bool TryFormatField(in T field, Span<char> tupleSpan, Span<char> fieldSpan, ref int charsWritten)
        {
            if (!field.TryFormat(fieldSpan, out int fieldCharsWritten, default, null))
            {
                charsWritten = 0;
                return false;
            }

            fieldSpan[..fieldCharsWritten].CopyTo(tupleSpan.Slice(charsWritten, fieldCharsWritten));
            charsWritten += fieldCharsWritten;

            return true;
        }
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
    public static bool AllocateMemoryAndCheckEquality<TValue, TType, TComparer>(int slots, in TType left, in TType right)  // 'left' and 'right' are passed using 'in' to avoid defensive copying.
        where TValue : unmanaged
        where TType : struct
        where TComparer : IMemoryComparer<TValue, TType>
    {
        int totalSlots = 2 * slots;  // 2x because we need to allocate stack memory for 'left' and 'right'

        if (IsStackSafe<TValue>(totalSlots))  
        {
            Span<TValue> leftSpan = stackalloc TValue[slots];
            Span<TValue> rightSpan = stackalloc TValue[slots];

            return TComparer.Equals(in left, ref leftSpan, in right, ref rightSpan);
        }

        if (IsPoolSafe<TValue>(totalSlots))
        {
            TValue[] buffer = ArrayPool<TValue>.Shared.Rent(totalSlots);

            Span<TValue> leftSpan = new(buffer, 0, slots);
            Span<TValue> rightSpan = new(buffer, slots, slots);

            // since 'ArrayPool.Shared' could be used from user-code, we need to be sure that the Span<TValue>(s) are cleared before handing them out.
            leftSpan.Clear();
            rightSpan.Clear();

            bool result = TComparer.Equals(in left, ref leftSpan, in right, ref rightSpan);
            ArrayPool<TValue>.Shared.Return(buffer);  // no need to clear the array, since it will be cleared by the Span<TValue>(s).

            return result;
        }

        Span<TValue> _leftSpan = new TValue[slots];
        Span<TValue> _rightSpan = new TValue[slots];

        return TComparer.Equals(in left, ref _leftSpan, in right, ref _rightSpan);
    }

    public static void AllocateMemory<T>(int slots, IMemoryConsumer<T> consumer) 
        where T : unmanaged
    {
        scoped Span<T> buffer;

        if (IsStackSafe<T>(slots))
        {
            buffer = stackalloc T[slots];
            consumer.Consume(ref buffer);

            return;
        }

        if (IsPoolSafe<T>(slots))
        {
            T[] pooledArray = ArrayPool<T>.Shared.Rent(slots);
            buffer = pooledArray.AsSpan();

            consumer.Consume(ref buffer);

            ArrayPool<T>.Shared.Return(pooledArray);

            return;
        }

        T[] array = new T[slots];
        buffer = array.AsSpan();

        consumer.Consume(ref buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TOut CastAs<TIn, TOut>(in TIn value) // 'value' is passed using 'in' to avoid defensive copying.
       where TIn : struct
       where TOut : struct
       => ref Unsafe.As<TIn, TOut>(ref Unsafe.AsRef(in value));

    /// <summary>
    /// Checks wether it is safe to allocate <paramref name="slots"/> in the stack.
    /// <para><i>It is good practice not to allocate more than 1 kilobyte of memory on the stack</i></para>
    /// </summary>
    /// <typeparam name="T">The type to allocate memory for, which can not contain any references.</typeparam>
    /// <param name="slots">The number of memory slots to allocate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStackSafe<T>(int slots) where T : unmanaged
        => slots * Unsafe.SizeOf<T>() <= 1024;

    /// <summary>
    /// Checks wether it is safe to allocate <paramref name="slots"/> from the <see cref="ArrayPool{T}"/>.
    /// <para><i><see langword="1,048,576"/> is the maximum array length of <see cref="ArrayPool{T}.Shared"/></i></para>
    /// </summary>
    /// <typeparam name="T">The type to allocate memory for.</typeparam>
    /// <param name="slots">The number of memory slots to allocate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPoolSafe<T>(int slots) => slots <= 1048576;
}