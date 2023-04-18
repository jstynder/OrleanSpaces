﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using OrleanSpaces.Tuples;
using OrleanSpaces.Tuples.Typed;

[ShortRunJob] //TODO: Remove
[MemoryDiagnoser]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.Declared)] 
public class BoolTupleBenchmarks
{
    private const int iterations = 100_000;

    #region Instantiation

    [BenchmarkCategory("Instantiation"), Benchmark]
    public void InstantiateSpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
            _ = new SpaceTuple(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);
    }

    [BenchmarkCategory("Instantiation"), Benchmark]
    public void InstantiateBoolTuple()
    {
        for (int i = 0; i < iterations; i++)
            _ = new BoolTuple(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);
    }

    #endregion

    #region Equality

    [BenchmarkCategory("Equality", "XS"), Benchmark]
    public void XS_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "XS"), Benchmark]
    public void XS_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "S"), Benchmark]
    public void S_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(true, true, true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "S"), Benchmark]
    public void S_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(true, true, true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "M"), Benchmark]
    public void M_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(true, true, true, true, true, true, true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "M"), Benchmark]
    public void M_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(true, true, true, true, true, true, true, true);
            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "L"), Benchmark]
    public void L_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "L"), Benchmark]
    public void L_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);


            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "XL"), Benchmark]
    public void XL_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "XL"), Benchmark]
    public void XL_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "XXL"), Benchmark]
    public void XXL_SpaceTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality", "XXL"), Benchmark]
    public void XXL_BoolTuple()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    #endregion

    #region Equality Type

    [BenchmarkCategory("Equality Type", "SpaceTuple"), Benchmark]
    public void SpaceTupleEquality()
    {
        for (int i = 0; i < iterations; i++)
        {
            SpaceTuple tuple = new(
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true,
               true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality Type", "Sequential BoolTuple"), Benchmark]
    public void SequentialBoolTupleEquality()
    {
        for (int i = 0; i < iterations; i++)
        {
            SequentialBoolTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    [BenchmarkCategory("Equality Type", "Parallel BoolTuple"), Benchmark]
    public void ParallelBoolTupleEquality()
    {
        for (int i = 0; i < iterations; i++)
        {
            BoolTuple tuple = new(
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true,
                true, true, true, true, true, true, true, true);

            tuple.Equals(tuple);
        }
    }

    private readonly struct SequentialBoolTuple : ISpaceTuple<bool, SequentialBoolTuple>
    {
        private readonly bool[] fields;

        public bool this[int index] => fields[index];
        public int Length => fields.Length;

        public SequentialBoolTuple() : this(Array.Empty<bool>()) { }
        public SequentialBoolTuple(params bool[] fields) => this.fields = fields;

        public static bool operator ==(SequentialBoolTuple left, SequentialBoolTuple right) => left.Equals(right);
        public static bool operator !=(SequentialBoolTuple left, SequentialBoolTuple right) => !(left == right);

        public override bool Equals(object obj) => obj is SequentialBoolTuple tuple && Equals(tuple);
        public bool Equals(SequentialBoolTuple other) => this.SequentialEquals(other);

        public int CompareTo(SequentialBoolTuple other) => Length.CompareTo(other.Length);

        public override int GetHashCode() => fields.GetHashCode();

        public override string ToString() => $"({string.Join(", ", fields)})";
    }

    #endregion
}
