﻿namespace OrleanSpaces.Proxies;

internal class ChannelProxy : ISpaceChannelProxy
{
    private readonly SpaceAgent agent;

    private bool initialized;
    private static readonly SemaphoreSlim semaphore = new(1, 1);

    public ChannelProxy(SpaceAgent agent)
    {
        this.agent = agent ?? throw new ArgumentNullException(nameof(agent));
    }

    public async Task<ISpaceChannel> OpenAsync()
    {
        await semaphore.WaitAsync();
        try
        {
            if (!initialized)
            {
                await agent.InitAsync();
                initialized = true;
            }
        }
        finally
        {
            semaphore.Release();
        }

        return agent;
    }
}