﻿using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using OrleanSpaces.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OrleanSpaces;

internal interface ISpaceGrain : IGrainWithGuidKey
{
    ValueTask<Guid> ListenAsync();

    Task WriteAsync(SpaceTuple tuple);
    ValueTask<SpaceTuple> PeekAsync(SpaceTemplate template);
    Task<SpaceTuple> PopAsync(SpaceTemplate template);
    ValueTask<IEnumerable<SpaceTuple>> ScanAsync(SpaceTemplate template);
    ValueTask<int> CountAsync(SpaceTemplate? template);
}

internal sealed class SpaceGrain : Grain, ISpaceGrain
{
    private readonly IPersistentState<TupleSpace> space;

    [AllowNull] private IAsyncStream<ITuple> stream;

    public SpaceGrain([PersistentState("TupleSpace", Constants.TupleSpaceStore)] IPersistentState<TupleSpace> space)
    {
        this.space = space ?? throw new ArgumentNullException(nameof(space));
    }

    public override Task OnActivateAsync()
    {
        var provider = GetStreamProvider(Constants.PubSubProvider);
        stream = provider.GetStream<ITuple>(this.GetPrimaryKey(), Constants.TupleStream);

        return base.OnActivateAsync();
    }

    public ValueTask<Guid> ListenAsync() => new(stream.Guid);

    public async Task WriteAsync(SpaceTuple tuple)
    {
        if (tuple.IsPassive)
        {
            throw new ArgumentException("Passive tuples are not allowed to be writen _in the tuple space.");
        }

        space.State.Tuples.Add(tuple);

        await space.WriteStateAsync();
        await stream.OnNextAsync(tuple);
    }

    public ValueTask<SpaceTuple> PeekAsync(SpaceTemplate template)
    {
        IEnumerable<SpaceTuple> tuples = space.State.Tuples
            .Where(x => x.Length == template.Length)
            .Select(x => (SpaceTuple)x);

        foreach (var tuple in tuples)
        {
            if (template.IsSatisfiedBy(tuple))
            {
                return new(tuple);
            }
        }

        return new(SpaceTuple.Passive);
    }

    public async Task<SpaceTuple> PopAsync(SpaceTemplate template)
    {
        var tupleDtos = space.State.Tuples.Where(x => x.Length == template.Length);

        foreach (var dto in tupleDtos)
        {
            SpaceTuple tuple = dto;

            if (template.IsSatisfiedBy(tuple))
            {
                space.State.Tuples.Remove(dto);

                await space.WriteStateAsync();
                await stream.OnNextAsync(template);

                if (space.State.Tuples.Count == 0)
                {
                    await stream.OnNextAsync(SpaceUnit.Null);
                }

                return tuple;
            }
        }

        return SpaceTuple.Passive;
    }

    public ValueTask<IEnumerable<SpaceTuple>> ScanAsync(SpaceTemplate template)
    {
        List<SpaceTuple> results = new();

        IEnumerable<SpaceTuple> tuples = space.State.Tuples
            .Where(x => x.Length == template.Length)
            .Select(x => (SpaceTuple)x);

        foreach (var tuple in tuples)
        {
            if (template.IsSatisfiedBy(tuple))
            {
                results.Add(tuple);
            }
        }

        return new(results);
    }

    public ValueTask<int> CountAsync(SpaceTemplate? template)
    {
        if (template == null)
        {
            return new(space.State.Tuples.Count);
        }

        return new(space.State.Tuples.Count(tuple => ((SpaceTemplate)template).IsSatisfiedBy(tuple)));
    }
}

[Serializable]
internal sealed class TupleSpace
{
    public List<SpaceTupleDto> Tuples { get; set; } = new();

    [Serializable]
    public class SpaceTupleDto
    {
        public List<object> Fields { get; set; } = new();
        public int Length => Fields.Count;

        public static implicit operator SpaceTupleDto(SpaceTuple tuple)
        {
            SpaceTupleDto dto = new();

            for (int i = 0; i < tuple.Length; i++)
            {
                dto.Fields.Add(tuple[i]);
            }

            return dto;
        }

        public static implicit operator SpaceTuple(SpaceTupleDto dto) =>
            new(dto.Fields.ToArray());
    }
}