﻿using OrleanSpaces.Observers;
using OrleanSpaces.Primitives;

namespace OrleanSpaces.Tests;

public class TestObserver : ISpaceObserver
{
    public SpaceTuple LastReceived { get; private set; }
    public bool SpaceEmptiedReceived { get; private set; }

    public virtual Task OnTupleAsync(SpaceTuple tuple)
    {
        LastReceived = tuple;
        return Task.CompletedTask;
    }

    public Task OnEmptySpaceAsync()
    {
        SpaceEmptiedReceived = true;
        return Task.CompletedTask;
    }
}

public class ThrowingTestObserver : TestObserver
{
    public override Task OnTupleAsync(SpaceTuple tuple)
    {
        throw new Exception("Test");
    }
}