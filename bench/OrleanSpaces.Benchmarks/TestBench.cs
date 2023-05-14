﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.CodeAnalysis.Text;
using OrleanSpaces.Tuples;
using OrleanSpaces.Tuples.Typed;
using System.Drawing;
using System.Numerics;

[ShortRunJob]
[MemoryDiagnoser]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.Declared)]
public class TestBench
{
    [Params(1_000)]//, 10_000, 100_000, 1_000_000)]
    public int Iterations { get; set; }

    private static readonly Int128 left = Int128.MaxValue;

    [Benchmark]
    public void Test()
    {
        
    }
}