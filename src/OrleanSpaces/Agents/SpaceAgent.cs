﻿using OrleanSpaces.Tuples;
using OrleanSpaces.Helpers;
using System.Threading.Channels;
using OrleanSpaces.Channels;
using OrleanSpaces.Registries;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace OrleanSpaces.Agents;

internal sealed class SpaceAgent : ISpaceAgent, ISpaceRouter<SpaceTuple, SpaceTemplate>
{
    private readonly static object lockObj = new();
    private readonly Guid agentId = Guid.NewGuid();
    private readonly SpaceClientOptions options;
    private readonly EvaluationChannel<SpaceTuple> evaluationChannel;
    private readonly ObserverRegistry<SpaceTuple> observerRegistry;
    private readonly CallbackRegistry callbackRegistry;
    private readonly ImmutableArray<StoreTuple<SpaceTuple>> tuples = ImmutableArray<StoreTuple<SpaceTuple>>.Empty; // chosen for thread safety

    [AllowNull] private IStoreDirector<SpaceTuple> director;
    private Channel<SpaceTuple>? streamChannel;
   
    public SpaceAgent(
        SpaceClientOptions options,
        EvaluationChannel<SpaceTuple> evaluationChannel,
        ObserverRegistry<SpaceTuple> observerRegistry,
        CallbackRegistry callbackRegistry)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.evaluationChannel = evaluationChannel ?? throw new ArgumentNullException(nameof(evaluationChannel));
        this.observerRegistry = observerRegistry ?? throw new ArgumentNullException(nameof(observerRegistry));
        this.callbackRegistry = callbackRegistry ?? throw new ArgumentNullException(nameof(callbackRegistry));
    }

    #region ISpaceRouter

    async ValueTask ISpaceRouter<SpaceTuple, SpaceTemplate>.RouteDirector(IStoreDirector<SpaceTuple> director)
    {
        if (options.LoadSpaceContentsUponStartup)
        {
            var tuples = await director.GetAll();
            foreach (var tuple in tuples)
            {
                this.tuples.Add(tuple);
            }
        }

        this.director = director;
    }

    async ValueTask ISpaceRouter<SpaceTuple, SpaceTemplate>.RouteAction(TupleAction<SpaceTuple> action)
    {
        if (action.AgentId != agentId)
        {
            switch (action.Type)
            {
                case TupleActionType.Insert:
                    {
                        tuples.Add(action.StoreTuple);
                        await streamChannel.WriteIfNotNull(action.StoreTuple.Tuple);
                    }
                    break;
                case TupleActionType.Remove:
                    tuples.Remove(action.StoreTuple);
                    break;
                case TupleActionType.Clear:
                    tuples.Clear();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    Task ISpaceRouter<SpaceTuple, SpaceTemplate>.RouteTuple(SpaceTuple tuple) => WriteAsync(tuple);
    async ValueTask ISpaceRouter<SpaceTuple, SpaceTemplate>.RouteTemplate(SpaceTemplate template) => await PopAsync(template);

    #endregion

    #region ISpaceAgent

    public Guid Subscribe(ISpaceObserver<SpaceTuple> observer)
        => observerRegistry.Add(observer);

    public void Unsubscribe(Guid observerId)
        => observerRegistry.Remove(observerId);

    public async Task WriteAsync(SpaceTuple tuple)
    {
        ThrowHelpers.EmptyTuple(tuple);

        Guid storeId = await director.Insert(new(agentId, new(Guid.Empty, tuple), TupleActionType.Insert));
        await streamChannel.WriteIfNotNull(tuple);

        tuples.Add(new(storeId, tuple));
    }

    public async ValueTask EvaluateAsync(Func<Task<SpaceTuple>> evaluation)
    {
        if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));
        await evaluationChannel.Writer.WriteAsync(evaluation);
    }

    public ValueTask<SpaceTuple> PeekAsync(SpaceTemplate template)
    {
        var tuple = tuples.FirstOrDefault(x => template.Matches(x.Tuple));
        return new(tuple.Tuple);
    }

    public async ValueTask PeekAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback)
    {
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        var tuple = tuples.FirstOrDefault(x => template.Matches(x.Tuple));
        if (tuple.Tuple.IsEmpty)
        {
            callbackRegistry.Add(template, new(callback, false));
            return;
        }

        await callback(tuple.Tuple);
    }

    public async ValueTask<SpaceTuple> PopAsync(SpaceTemplate template)
    {
        var tuple = tuples.FirstOrDefault(x => template.Matches(x.Tuple));

        if (!tuple.Tuple.IsEmpty)
        {
            await director.Remove(new(agentId, tuple, TupleActionType.Remove));
            tuples.Remove(tuple);
        }

        return tuple.Tuple;
    }

    public async ValueTask PopAsync(SpaceTemplate template, Func<SpaceTuple, Task> callback)
    {
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        var tuple = tuples.FirstOrDefault(x => template.Matches(x.Tuple));
        if (tuple.Tuple.IsEmpty)
        {
            callbackRegistry.Add(template, new(callback, true));
            return;
        }

        await callback(tuple.Tuple);
        await director.Remove(new(agentId, tuple, TupleActionType.Remove));

        tuples.Remove(tuple);
    }

    public ValueTask<IEnumerable<SpaceTuple>> ScanAsync(SpaceTemplate template)
    {
        List<SpaceTuple> result = new();

        foreach (var tuple in tuples)
        {
            if (template.Matches(tuple.Tuple))
            {
                result.Add(tuple.Tuple);
            }
        }

        return new(result);
    }

    public async IAsyncEnumerable<SpaceTuple> PeekAsync()
    {
        lock (lockObj)
        {
            if (streamChannel is null)
            {
                streamChannel = Channel.CreateUnbounded<SpaceTuple>(new()
                {
                    SingleReader = !options.AllowMultipleAgentStreamConsumers,
                    SingleWriter = true
                });


                foreach (var tuple in tuples)
                {
                    _ = streamChannel.Writer.TryWrite(tuple.Tuple);  // will always be able to write to the channel
                }
            }
        }

        await foreach (SpaceTuple tuple in streamChannel.Reader.ReadAllAsync())
        {
            yield return tuple;
        }
    }

    public ValueTask<int> CountAsync() => new(tuples.Length);

    public async Task ReloadAsync()
    {
        var tuples = await director.GetAll();
        this.tuples.Clear();

        foreach (var tuple in tuples)
        {
            this.tuples.Add(tuple);
        }
    }

    public async Task ClearAsync()
    {
        await director.RemoveAll(agentId);
        tuples.Clear();
    }

    #endregion
}