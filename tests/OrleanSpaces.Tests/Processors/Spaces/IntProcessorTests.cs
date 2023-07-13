﻿using OrleanSpaces.Channels;
using OrleanSpaces.Processors.Spaces;
using OrleanSpaces.Tuples.Specialized;

namespace OrleanSpaces.Tests.Processors.Spaces;

public class IntProcessorTests : IClassFixture<ClusterFixture>
{
    private readonly Fixture fixture;

    public IntProcessorTests(ClusterFixture clusterFixture) => fixture = new(clusterFixture);

    [Fact]
    public async Task Should_Forward_On_Insert_Action()
    {
        TupleAction<IntTuple> action = new(Guid.NewGuid(), new(1), TupleActionType.Insert);

        await fixture.Processor.OnNextAsync(action);

        IntTuple callbackResult = await fixture.CallbackChannel.Reader.ReadAsync(default);
        TupleAction<IntTuple> observerResult = await fixture.ObserverChannel.Reader.ReadAsync(default);

        Assert.Equal(action.AgentId, fixture.Bridge.LastAction.AgentId);
        Assert.Equal(action.Tuple, fixture.Bridge.LastAction.Tuple);
        Assert.Equal(action.Type, fixture.Bridge.LastAction.Type);
        Assert.Equal(callbackResult, action.Tuple);
        Assert.Equal(observerResult, action);
    }

    [Fact]
    public async Task Should_Forward_On_Remove_Action()
    {
        TupleAction<IntTuple> action = new(Guid.NewGuid(), new(1), TupleActionType.Remove);

        await fixture.Processor.OnNextAsync(action);

        TupleAction<IntTuple> observerResult = await fixture.ObserverChannel.Reader.ReadAsync(default);

        Assert.Equal(action.AgentId, fixture.Bridge.LastAction.AgentId);
        Assert.Equal(action.Tuple, fixture.Bridge.LastAction.Tuple);
        Assert.Equal(action.Type, fixture.Bridge.LastAction.Type);
        Assert.Equal(observerResult, action);
    }

    [Fact]
    public async Task Should_Forward_On_Clear_Action()
    {
        TupleAction<IntTuple> action = new(Guid.NewGuid(), new(1), TupleActionType.Clear);

        await fixture.Processor.OnNextAsync(action);

        TupleAction<IntTuple> observerResult = await fixture.ObserverChannel.Reader.ReadAsync(default);

        Assert.Equal(action.AgentId, fixture.Bridge.LastAction.AgentId);
        Assert.Equal(action.Tuple, fixture.Bridge.LastAction.Tuple);
        Assert.Equal(action.Type, fixture.Bridge.LastAction.Type);
        Assert.Equal(observerResult, action);
    }

    private class Fixture
    {
        internal IntProcessor Processor { get; }

        internal TestAgentProcessorBridge<IntTuple> Bridge { get; }
        internal ObserverChannel<IntTuple> ObserverChannel { get; }
        internal CallbackChannel<IntTuple> CallbackChannel { get; }

        public Fixture(ClusterFixture fixture)
        {
            Bridge = new();
            ObserverChannel = new();
            CallbackChannel = new();
            Processor = new(fixture.Client, Bridge, ObserverChannel, CallbackChannel);
        }
    }
}