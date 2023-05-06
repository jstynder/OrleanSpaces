﻿namespace OrleanSpaces.Tuples;

internal struct ValueTupleFormatter<T, TSelf> : IBufferConsumer<char>
    where T : struct, ISpanFormattable
    where TSelf : IValueTuple<T, TSelf>
{
    private readonly int maxFieldCharLength;
    private readonly IValueTuple<T, TSelf> tuple;

    private int charsWritten;

    public ValueTupleFormatter(IValueTuple<T, TSelf> tuple, int maxFieldCharLength, ref int charsWritten)
    {
        this.tuple = tuple;
        this.maxFieldCharLength = maxFieldCharLength;
        this.charsWritten = charsWritten;
    }

    public bool Consume(ref Span<char> buffer)
    {
        Span<char> fieldSpan = stackalloc char[maxFieldCharLength]; // its safe to allocate memory on the stack because the maxFieldCharLength is a constant on all tuples, and has a finite value: [2 bytes (since 'char') * maxFieldCharLength <= 1024 bytes]

        buffer.Clear();
        fieldSpan.Clear();

        int tupleLength = tuple.Length;
        if (tupleLength == 1)
        {
            buffer[charsWritten++] = '(';

            if (!TryFormatField(in tuple[0], fieldSpan, buffer, ref charsWritten))
            {
                return false;
            }

            buffer[charsWritten++] = ')';

            return true;
        }

        buffer[charsWritten++] = '(';

        for (int i = 0; i < tupleLength; i++)
        {
            if (i > 0)
            {
                buffer[charsWritten++] = ',';
                buffer[charsWritten++] = ' ';

                fieldSpan.Clear();
            }

            if (!TryFormatField(in tuple[i], fieldSpan, buffer, ref charsWritten))
            {
                return false;
            }
        }

        buffer[charsWritten++] = ')';

        return true;
    }

    private static bool TryFormatField(in T field, Span<char> fieldSpan, Span<char> destination, ref int charsWritten)
    {
        if (!field.TryFormat(fieldSpan, out int fieldCharsWritten, default, null))
        {
            charsWritten = 0;
            return false;
        }

        fieldSpan[..fieldCharsWritten].CopyTo(destination.Slice(charsWritten, fieldCharsWritten));
        charsWritten += fieldCharsWritten;

        return true;
    }
}