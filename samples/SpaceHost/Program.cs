﻿using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using OrleanSpaces;
using Microsoft.Extensions.Hosting;
using Orleans;

var host = new SiloHostBuilder()
    .UseLocalhostClustering()
    .AddSimpleMessageStreamProvider(StreamNames.PubSubProvider)
    .AddMemoryGrainStorage(StreamNames.PubSubStore)
    .AddMemoryGrainStorage(StorageNames.TupleSpaceStore)
    .AddTupleSpace()
    .ConfigureLogging(builder => builder.AddConsole())
    .Build();

await host.StartAsync();

Console.WriteLine("\nPress any key to terminate...\n");
Console.ReadLine();

await host.StopAsync();