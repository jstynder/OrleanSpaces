﻿using OrleanSpaces.Observers;
using OrleanSpaces.Primitives;

namespace OrleanSpaces.Tests.Observers;

public partial class ObserverTests 
{
    private static readonly SpaceTuple tuple = new(1);
    private static readonly SpaceTemplate template = new(1);

    private static Func<OrleanSpaces.Observers.DynamicObserver, Task> Expansion =>
        async (observer) => await observer.HandleAsync(tuple);

    private static Func<OrleanSpaces.Observers.DynamicObserver, Task> Contraction =>
        async (observer) => await observer.HandleAsync(template);

    private static Func<OrleanSpaces.Observers.DynamicObserver, Task> Flattening =>
        async (observer) => await observer.HandleAsync(SpaceUnit.Null);

    private static Func<OrleanSpaces.Observers.DynamicObserver, Task> Everything =>
        async (observer) =>
        {
            await Expansion(observer);
            await Contraction(observer);
            await Flattening(observer);
        };


    [Fact]
    public async Task Should_Invoke_All_For_EverythingObserver()
    {
        EverythingObserver observer = new();

        await Everything(observer);

        Assert.Equal(tuple, observer.LastTuple);
        Assert.Equal(template, observer.LastTemplate);
        Assert.True(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Invoke_Only_OnExpansion_For_ExpansionsObserver()
    {
        ExpansionsObserver observer = new();

        await Everything(observer);

        Assert.Equal(tuple, observer.LastTuple);
        Assert.NotEqual(template, observer.LastTemplate);
        Assert.False(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Invoke_Only_OnContraction_For_ContractionsObserver()
    {
        ContractionsObserver observer = new();

        await Everything(observer);

        Assert.NotEqual(tuple, observer.LastTuple);
        Assert.Equal(template, observer.LastTemplate);
        Assert.False(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Invoke_OnExpansion_And_OnContraction_For_CombinedObserver()
    {
        CombinedObserver observer = new();

        await Everything(observer);

        Assert.Equal(tuple, observer.LastTuple);
        Assert.Equal(template, observer.LastTemplate);
        Assert.False(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Invoke_Only_OnFlattening_For_FlatteningsObserver()
    {
        FlatteningsObserver observer = new();

        await Everything(observer);

        Assert.NotEqual(tuple, observer.LastTuple);
        Assert.NotEqual(template, observer.LastTemplate);
        Assert.True(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Invoke_Nothing_For_NothingObserver()
    {
        NothingObserver observer = new();
        await Everything(observer);

        Assert.NotEqual(tuple, observer.LastTuple);
        Assert.NotEqual(template, observer.LastTemplate);
        Assert.False(observer.LastFlattening);
    }

    [Fact]
    public async Task Should_Be_Able_To_Change_Interests_During_Runtime()
    {
        DynamicObserver observer = new();

        await Flattening(observer);
        Assert.False(observer.LastFlattening);

        await Contraction(observer);
        Assert.NotEqual(template, observer.LastTemplate);

        await Expansion(observer);
        Assert.Equal(tuple, observer.LastTuple);

        observer.Reset();

        await Contraction(observer);
        Assert.Equal(template, observer.LastTemplate);

        await Flattening(observer);
        Assert.True(observer.LastFlattening);

        await Expansion(observer);
        Assert.NotEqual(tuple, observer.LastTuple);
    }

    #region Observers

    private class EverythingObserver : BaseObserver
    {
        public EverythingObserver() => Interested(In.Everything);
    }

    private class ExpansionsObserver : BaseObserver
    {
        public ExpansionsObserver() => Interested(In.Expansions);
    }

    private class ContractionsObserver : BaseObserver
    {
        public ContractionsObserver() => Interested(In.Contractions);
    }

    private class CombinedObserver : BaseObserver
    {
        public CombinedObserver() => Interested(In.Expansions | In.Contractions);
    }

    private class FlatteningsObserver : BaseObserver
    {
        public FlatteningsObserver() => Interested(In.Flattening);
    }

    private class NothingObserver : BaseObserver
    {
        public NothingObserver() => Interested(In.Nothing);
    }

    private class DynamicObserver : BaseObserver
    {
        public DynamicObserver() => Interested(In.Expansions);

        public void Reset()
        {
            LastTuple = new();
            LastTemplate = new();
            LastFlattening = false;
        }

        public override Task OnExpansionAsync(SpaceTuple tuple, CancellationToken cancellationToken)
        {
            base.OnExpansionAsync(tuple, cancellationToken);
            Interested(In.Contractions);

            return Task.CompletedTask;
        }

        public override Task OnContractionAsync(SpaceTemplate template, CancellationToken cancellationToken)
        {
            base.OnContractionAsync(template, cancellationToken);
            Interested(In.Flattening);

            return Task.CompletedTask;
        }

        public override Task OnFlatteningAsync(CancellationToken cancellationToken)
        {
            base.OnFlatteningAsync(cancellationToken);
            Interested(In.Nothing);

            return Task.CompletedTask;
        }
    }

    private class BaseObserver : OrleanSpaces.Observers.DynamicObserver
    {
        public SpaceTuple LastTuple { get; protected set; } = new();
        public SpaceTemplate LastTemplate { get; protected set; } = new();
        public bool LastFlattening { get; protected set; }

        public override Task OnExpansionAsync(SpaceTuple tuple, CancellationToken cancellationToken)
        {
            LastTuple = tuple;
            return Task.CompletedTask;
        }

        public override Task OnContractionAsync(SpaceTemplate template, CancellationToken cancellationToken)
        {
            LastTemplate = template;
            return Task.CompletedTask;
        }

        public override Task OnFlatteningAsync(CancellationToken cancellationToken)
        {
            LastFlattening = true;
            return Task.CompletedTask;
        }
    }


    #endregion
}