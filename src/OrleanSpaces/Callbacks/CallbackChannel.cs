﻿using OrleanSpaces.Tuples;
using System.Threading.Channels;

namespace OrleanSpaces.Callbacks;

internal sealed class CallbackChannel<TTuple, TTemplate> : IConsumable
    where TTuple : ISpaceTuple
    where TTemplate : ISpaceTemplate
{
    public bool IsBeingConsumed { get; set; }

    private readonly Channel<CallbackPair<TTuple, TTemplate>> channel =
        Channel.CreateUnbounded<CallbackPair<TTuple, TTemplate>>(new() { SingleReader = true });

    public ChannelReader<CallbackPair<TTuple, TTemplate>> Reader => channel.Reader;
    public ChannelWriter<CallbackPair<TTuple, TTemplate>> Writer => channel.Writer;
}