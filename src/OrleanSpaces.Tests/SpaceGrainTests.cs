﻿using Orleans;
using Orleans.Streams;
using OrleanSpaces.Primitives;
using System.Runtime.CompilerServices;

namespace OrleanSpaces.Tests;

// The majority of grain methods are tested via the agent tests. Here we test only some edge cases.
public class SpaceGrainTests : IAsyncLifetime, IClassFixture<ClusterFixture>
{
    private readonly IClusterClient client;
    private readonly ISpaceGrain grain;
    private readonly AsyncObserver observer;

    private IAsyncStream<ITuple> stream;
    private Guid streamId;

    public SpaceGrainTests(ClusterFixture fixture)
    {
        observer = new();
        client = fixture.Client;
        grain = fixture.Client.GetGrain<ISpaceGrain>(Guid.Empty);
    }

    public async Task InitializeAsync()
    {
        streamId = await grain.ListenAsync();
        var provider = client.GetStreamProvider(StreamNames.PubSubProvider);
        stream = provider.GetStream<ITuple>(streamId, StreamNamespaces.Tuple);
        await stream.SubscribeAsync(observer);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Should_Get_Stream_Id()
    {
        Guid id = await grain.ListenAsync();
        Assert.Equal(streamId, id);
    }

    [Fact]
    public async Task Should_Notify_Observer_On_Empty_Space()
    {
        SpaceTuple tuple1 = new(1);
        SpaceTuple tuple2 = new((1, "a"));

        await grain.WriteAsync(tuple1);
        await grain.WriteAsync(tuple2);

        _ = await grain.PopAsync(tuple1);
        Assert.False(observer.SpaceEmptiedReceived);

        _ = await grain.PopAsync(tuple2);
        Assert.True(observer.SpaceEmptiedReceived);

        // Clear for next test
        await grain.PopAsync(tuple1);
        await grain.PopAsync(tuple2);

        observer.Reset();
    }

    [Theory]
    [ClassData(typeof(TupleGenerator))]
    public async Task Should_Notify_Observer_On_Added_And_Removed(SpaceTuple tuple)
    {
        // Add
        await grain.WriteAsync(tuple);

        // Remove
        SpaceTemplate template = tuple;
        await grain.PopAsync(template);

        Assert.Equal(tuple, observer.LastTuple);
        Assert.Equal(template, observer.LastTemplate);

        // Clear for next test
        observer.Reset();
    }

    private class AsyncObserver : IAsyncObserver<ITuple>
    {
        public SpaceTuple LastTuple { get; private set; } = new();
        public SpaceTemplate LastTemplate { get; private set; } = new();
        public bool SpaceEmptiedReceived { get; private set; }

        public Task OnNextAsync(ITuple tuple, StreamSequenceToken token)
        {
            if (tuple is SpaceTuple spaceTuple)
            {
                LastTuple = spaceTuple;
            }
            else if (tuple is SpaceTemplate template)
            {
                LastTemplate = template;
            }
            else if (tuple is SpaceUnit)
            {
                SpaceEmptiedReceived = true;
            }

            return Task.CompletedTask;
        }

        public Task OnCompletedAsync() => Task.CompletedTask;
        public Task OnErrorAsync(Exception ex) => Task.CompletedTask;

        public void Reset()
        {
            LastTuple = SpaceTuple.Passive;
            SpaceEmptiedReceived = false;
        }
    }
}