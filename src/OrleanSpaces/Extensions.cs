﻿using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrleanSpaces.Internals.Functions;
using OrleanSpaces.Internals.Agents;

namespace OrleanSpaces;

public static class ClientExtensions
{
    public static ISpaceProvider GetSpaceProvider(this IGrainFactory factory)
        => factory.GetGrain<ISpaceProvider>(Guid.Empty);

    public static IClientBuilder ConfigureTupleSpace(this IClientBuilder builder)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ClientExtensions).Assembly).WithReferences());
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TupleFunctionSerializer>();
            services.AddSingleton<IOutgoingGrainCallFilter, TupleFunctionEvaluator>();
        });

        return builder;
    }

    public static async Task SubscribeAsync<TObserver>(this IClusterClient client, TObserver observer) 
        where TObserver : ISpaceObserver
    {
        var registry = client.ServiceProvider.GetRequiredService<ISpaceAgentRegistry>();
        var observerRef = await client.CreateObjectReference<TObserver>(observer);

        registry.Register(observerRef);
    }

    public static async Task UnsubscribeAsync<TObserver>(this IClusterClient client, TObserver observer)
        where TObserver : ISpaceObserver
    {
        var registry = client.ServiceProvider.GetRequiredService<ISpaceAgentRegistry>();
        var observerRef = await client.CreateObjectReference<TObserver>(observer);

        registry.Deregister(observerRef);
    }
}

public static class HostingExtensions
{ 
    public static ISiloBuilder ConfigureTupleSpace(this ISiloBuilder builder)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HostingExtensions).Assembly).WithReferences());
        builder.ConfigureServices(services => services.AddSiloComponents());

        return builder;
    }

    public static ISiloHostBuilder ConfigureTupleSpace(this ISiloHostBuilder builder)
    {
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HostingExtensions).Assembly).WithReferences());
        builder.ConfigureServices(services => services.AddSiloComponents());

        return builder;
    }

    private static void AddSiloComponents(this IServiceCollection services)
    {
        services.AddSingleton<TupleFunctionSerializer>();

        services.AddSingleton<ISpaceAgentNotifier, SpaceAgentManager>();
        services.AddSingleton<ISpaceAgentRegistry, SpaceAgentManager>();
        services.AddSingleton<IIncomingGrainCallFilter, MyFilter>();
    }
}

