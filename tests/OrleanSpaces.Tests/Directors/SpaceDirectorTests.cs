﻿using Orleans.Runtime;
using Orleans.Streams;
using OrleanSpaces.Tuples;
using OrleanSpaces.Grains.Directors;
using System.Diagnostics.CodeAnalysis;

namespace OrleanSpaces.Tests.Directors;

public class SpaceDirectorTests : IAsyncLifetime, IClassFixture<ClusterFixture>
{
    private readonly ISpaceDirector director;
    private readonly IClusterClient client;
    private readonly Guid agentId = Guid.NewGuid();
    private readonly TestStreamObserver<SpaceTuple> observer = new();

    [AllowNull] private IAsyncStream<TupleAction<SpaceTuple>> stream;

    public SpaceDirectorTests(ClusterFixture fixture)
    {
        client = fixture.Client;
        director = fixture.Client.GetGrain<ISpaceDirector>(Constants.RealmKey_Space);
    }

    public async Task InitializeAsync()
    {
        stream = client
            .GetStreamProvider(Constants.PubSubProvider)
            .GetStream<TupleAction<SpaceTuple>>(
                StreamId.Create(Constants.StreamName, Constants.RealmKey_Space));

        await stream.SubscribeAsync(observer);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [ClassData(typeof(SpaceTupleGenerator))]
    public async Task Should_Notify_Observer(SpaceTuple tuple)
    {
        observer.Reset();

        Guid storeId = await director.Insert(new(agentId, tuple.WithDefaultStore(), TupleActionType.Insert));
        await director.Remove(new(agentId, new(storeId, tuple), TupleActionType.Remove));
        await director.RemoveAll(agentId);

        while (observer.InvokedCount < 3)
        {
            await Task.Delay(1);
        }

        Assert.Equal(tuple, observer.LastExpansionTuple);
        Assert.Equal(tuple, observer.LastContractionTuple);
        Assert.True(observer.HasFlattened);
    }
}
