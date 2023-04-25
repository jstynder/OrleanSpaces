﻿using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OrleanSpaces.Tuples;

internal static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParallelEquals<T, TSelf>(this INumericValueTuple<T, TSelf> left, INumericValueTuple<T, TSelf> right, out bool result)
        where T : struct, INumber<T>
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

        result = ParallelEquals_New(left.Fields, right.Fields);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<T> PadWithZeros<T>(this Span<T> span)
        where T : struct, INumber<T>
    {
        Span<T> paddedSpan = new T[Vector<T>.Count];
        span.CopyTo(paddedSpan[..span.Length]);

        return paddedSpan;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParallelEquals<TIn, TOut>(this NumericMarshaller<TIn, TOut> marshaller, out bool result)
        where TIn : struct
        where TOut : struct, INumber<TOut>
    {
        result = false;

        if (marshaller.Left.Length != marshaller.Right.Length)
        {
            return true;
        }

        if (!marshaller.Left.IsVectorizable())
        {
            return false;
        }

        result = ParallelEquals(marshaller.Left, marshaller.Right);
        return true;
    }


    //TODO: Benchmark and use in all...

    /// <remarks><i>Ensure the <see cref="Span{T}.Length"/>(s) of <paramref name="left"/> and <paramref name="right"/> are equal beforehand.</i></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParallelEquals_New<T>(this Span<T> left, Span<T> right)
        where T : struct, INumber<T>
    {
        int length = left.Length;
        if (length == 0)
        {
            return true;  // no elements, therefor 'left' & 'right' are equal.
        }

        ref T iLeft = ref left.GetZeroIndex();
        ref T iRight = ref right.GetZeroIndex();

        if (length == 1)
        {
            return iLeft == iRight;  // avoiding overhead by doing a non-vectorized equality check, as its a single operation eitherway.
        }

        // we create Vector<T> instances from the Span<T>'s by reference, so we don't need two extra buffers to copy the elements into.
        ref Vector<T> vLeft = ref Transform<T, Vector<T>>(in iLeft);
        ref Vector<T> vRight = ref Transform<T, Vector<T>>(in iRight);
        
        int vCount = Vector<T>.Count;
        int vLength = length / vCount;

        if (vLength == 0)
        {
            vLength = 1;
        }

        int i = 0;

        for (; i < vLength; i++)
        {
            if (vLeft.Offset(i) != vRight.Offset(i))
            {
                return false;
            }
        }

        i *= vCount;
        
        int remainingLength = length - i;

        if (remainingLength == 0)
        {
            return true;  // means [length % vCount = 0] therefor all elements have been compared (in parallel), and none were different (otherwise 'false' would have been returned) 
        }

        if (remainingLength == 1)
        {
            return iLeft.Offset(i) == iRight.Offset(i);  // avoiding overhead by doing a non-vectorized equality check, as its a single operation eitherway.
        }

        iLeft = ref left.Slice(i, remainingLength).GetZeroIndex();
        iRight = ref right.Slice(i, remainingLength).GetZeroIndex();

        vLeft = ref Transform<T, Vector<T>>(in iLeft);    // vector will have [i + vCount - remainingLength] elements set to default(T)
        vRight = ref Transform<T, Vector<T>>(in iRight);  // vector will have [i + vCount - remainingLength] elements set to default(T)

        return vLeft == vRight;
    }

    /// <remarks><i>Ensure the <see cref="Span{T}.Length"/>(s) of <paramref name="left"/> and <paramref name="right"/> are equal beforehand.</i></remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ParallelEquals<T>(this Span<T> left, Span<T> right)
        where T : struct, INumber<T>
    {
        ref T iLeft = ref left.GetZeroIndex();
        ref T iRight = ref right.GetZeroIndex();

        int i = 0;
        int length = left.Length;

        int vCount = Vector<T>.Count;
        int vLength = left.Length / vCount;

        ref Vector<T> vLeft = ref Transform<T, Vector<T>>(in iLeft);
        ref Vector<T> vRight = ref Transform<T, Vector<T>>(in iRight);

        for (; i < vLength; i++)
        {
            if (vLeft.Offset(i) != vRight.Offset(i))
            {
                return false;
            }
        }

        i *= vCount;

        for (; i < length; i++)
        {
            if (iLeft.Offset(i) != iRight.Offset(i))
            {
                return false;
            }
        }

        return true;
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
    public static bool IsVectorizable<T>(this Span<T> span) where T : struct, INumber<T>
        => Vector.IsHardwareAccelerated && span.Length / Vector<T>.Count > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T GetZeroIndex<T>(this Span<T> span) where T : struct
        => ref MemoryMarshal.GetReference(span);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Offset<T>(this ref T source, int count) where T : struct
        => ref Unsafe.Add(ref source, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TOut Transform<TIn, TOut>(in TIn value)
        where TIn : struct
        where TOut : struct
        => ref Unsafe.As<TIn, TOut>(ref Unsafe.AsRef(in value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormatTuple<T, TSelf>(this IValueTuple<T, TSelf> tuple, int maxFieldCharLength, Span<char> destination, out int charsWritten)
        where T : struct, ISpanFormattable
        where TSelf : IValueTuple<T, TSelf>
    {
        charsWritten = 0;

        int tupleLength = tuple.Length;
        int totalLength = CalculateTotalLength(tupleLength, maxFieldCharLength);

        if (destination.Length < totalLength)
        {
            charsWritten = 0;
            return false;
        }

        if (tupleLength == 0)
        {
            destination[charsWritten++] = '(';
            destination[charsWritten++] = ')';

            return true;
        }

        Span<char> tupleSpan = stackalloc char[totalLength];
        Span<char> fieldSpan = stackalloc char[maxFieldCharLength];

        if (tupleLength == 1)
        {
            tupleSpan[charsWritten++] = '(';

            if (!TryFormatField(in tuple[0], tupleSpan, fieldSpan, ref charsWritten))
            {
                return false;
            }

            tupleSpan[charsWritten++] = ')';
            tupleSpan[..(charsWritten + 1)].CopyTo(destination);

            return true;
        }

        tupleSpan[charsWritten++] = '(';

        for (int i = 0; i < tupleLength; i++)
        {
            if (i > 0)
            {
                tupleSpan[charsWritten++] = ',';
                tupleSpan[charsWritten++] = ' ';

                fieldSpan.Clear();
            }

            if (!TryFormatField(in tuple[i], tupleSpan, fieldSpan, ref charsWritten))
            {
                return false;
            }
        }

        tupleSpan[charsWritten++] = ')';
        tupleSpan[..(charsWritten + 1)].CopyTo(destination);

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
        int separatorsCount = tupleLength == 0 ? 0 : 2 * (tupleLength - 1);
        int destinationSpanLength = tupleLength == 0 ? 2 : maxFieldCharLength * tupleLength;
        int totalLength = tupleLength == 0 ? 2 : destinationSpanLength + separatorsCount + 2;

        return totalLength;
    }
}