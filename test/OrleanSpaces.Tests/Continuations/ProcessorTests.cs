﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using OrleanSpaces.Continuations;
using OrleanSpaces.Observers;
using OrleanSpaces.Primitives;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;

namespace OrleanSpaces.Tests.Continuations;

public class ProcessorTests : IClassFixture<ProcessorTests.Fixture>
{
    private readonly ISpaceAgent agent;
    private readonly ContinuationChannel channel;

    public ProcessorTests(Fixture fixture)
    {
        agent = fixture.Agent;
        channel = fixture.Channel;
    }

    [Fact]
    public async Task Should_Write_Tuple_If_SpaceElement_Is_A_Tuple()
    {
        SpaceTuple tuple = SpaceTuple.Create("continue");

        await channel.Writer.WriteAsync(tuple);

        SpaceTuple peekedTuple = await agent.PeekAsync(SpaceTemplate.Create(tuple));

        Assert.False(peekedTuple.IsEmpty);
        Assert.Equal(tuple, peekedTuple);
    }

    [Fact]
    public async Task Should_Pop_Tuple_If_SpaceElement_Is_A_Template()
    {
        SpaceTuple tuple = SpaceTuple.Create("continue");
        await agent.WriteAsync(tuple);

        SpaceTemplate template = SpaceTemplate.Create(tuple);
        await channel.Writer.WriteAsync(template);

        SpaceTuple peekedTuple = await agent.PeekAsync(template);

        Assert.True(peekedTuple.IsEmpty);
        Assert.NotEqual(tuple, peekedTuple);
    }

    [Fact]
    public async Task Should_Throw_On_Unsupported_SpaceElement()
    {
        await Assert.ThrowsAsync<NotImplementedException>(async () => await channel.Writer.WriteAsync(new TestElement()));
    }


    public class Fixture : IAsyncLifetime
    {
        private readonly ContinuationProcessor processor;
        private readonly ISpaceChannel spaceChannel;

        internal ISpaceAgent Agent { get; private set; }
        internal ContinuationChannel Channel { get; }

        public Fixture()
        {
            spaceChannel = ;
            Channel = new();

            processor = new(spaceChannel, Channel, new NullLogger<ContinuationProcessor>());
        }

        public async Task InitializeAsync()
        {
            await processor.StartAsync(default);
            Agent = await spaceChannel.GetAsync();
        }

        public async Task DisposeAsync() => await processor.StopAsync(default);
    }

    private class TestElement : ISpaceElement
    {

    }

    //private class TestChannel : ISpaceChannel
    //{
    //    public Task<ISpaceAgent> GetAsync() => Task.FromResult(new TestAgent());

    //    private class TestAgent : ISpaceAgent
    //    {
    //        private readonly List<SpaceTuple> tuples = new();

    //        public Task WriteAsync(SpaceTuple tuple)
    //        {
    //            tuples.Add(tuple);
    //            return Task.CompletedTask;
    //        }

    //        public ValueTask<SpaceTuple> PeekAsync(SpaceTemplate template)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Task EvaluateAsync(Func<Task<SpaceTuple>> evaluation)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public ValueTask PeekAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Task<SpaceTuple> PopAsync(SpaceTemplate template)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public Task PopAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public ObserverRef Subscribe(ISpaceObserver observer)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public void Unsubscribe(ObserverRef @ref)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public ValueTask<IEnumerable<SpaceTuple>> ScanAsync(SpaceTemplate template)
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public ValueTask<int> CountAsync()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        public ValueTask<int> CountAsync(SpaceTemplate template)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }
    }
}