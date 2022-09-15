﻿using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;

namespace OrleanSpaces.Tests;

public class ClusterFixture : IDisposable
{
    private readonly TestCluster cluster;
    public IClusterClient Client { get; }

    public ClusterFixture()
    {
        cluster = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<TestSiloConfigurator>()
            .AddClientBuilderConfigurator<TestClientConfigurator>()
            .Build();

        cluster.Deploy();
        Client = cluster.Client;
    }

    public void Dispose() => cluster.StopAllSilos();

    private class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddSimpleMessageStreamProvider(Constants.PubSubProvider);
            siloBuilder.AddMemoryGrainStorage(Constants.PubSubStore);
            siloBuilder.AddMemoryGrainStorage(Constants.PersistenceStore);
            siloBuilder.AddTupleSpace();
        }
    }

    private class TestClientConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.AddSimpleMessageStreamProvider(Constants.PubSubProvider);
            clientBuilder.AddTupleSpace();
        }
    }
}
