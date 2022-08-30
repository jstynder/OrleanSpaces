﻿using Orleans;
using Orleans.Hosting;
using OrleanSpaces.Bridges;
using OrleanSpaces.Observers;
using OrleanSpaces.Callbacks;
using Microsoft.Extensions.DependencyInjection;
using OrleanSpaces.Continuations;
using OrleanSpaces.Evaluations;

namespace OrleanSpaces;

public static class Extensions
{
    public static ISiloBuilder AddTupleSpace(this ISiloBuilder builder) =>
        builder.ConfigureApplicationParts(parts =>
            parts.AddApplicationPart(typeof(Extensions).Assembly).WithReferences());
     
    public static ISiloHostBuilder AddTupleSpace(this ISiloHostBuilder builder) =>
        builder.ConfigureApplicationParts(parts =>
            parts.AddApplicationPart(typeof(Extensions).Assembly).WithReferences());

    public static IClientBuilder AddTupleSpace(this IClientBuilder builder) =>
        builder.ConfigureServices(services => services.AddClientServices());

    public static IServiceCollection AddTupleSpace(this IServiceCollection services, Func<IClusterClient>? clusterClientFactory = null)
    {
        services.AddSingleton(clusterClientFactory?.Invoke() ?? BuildDefaultClient());
        services.AddClientServices();

        return services;

        static IClusterClient BuildDefaultClient() =>
            new ClientBuilder()
                .UseLocalhostClustering()
                .AddSimpleMessageStreamProvider(StreamNames.PubSubProvider)
                .Build();
    }

    private static IServiceCollection AddClientServices(this IServiceCollection services)
    {
        services.AddSingleton<CallbackRegistry>();
        services.AddSingleton<ObserverRegistry>();

        services.AddSingleton<SpaceGrainBridge>();
        services.AddSingleton<ISpaceChannelProvider, SpaceChannelBridge>();

        services.AddHostedService<CallbackProcessor>();
        services.AddHostedService<EvaluationProcessor>();
        services.AddHostedService<ContinuationProcessor>();
        services.AddHostedService<ObserverProcessor>();

        return services;
    }
}