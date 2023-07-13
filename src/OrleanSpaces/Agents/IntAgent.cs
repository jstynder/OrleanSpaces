﻿using OrleanSpaces.Channels;
using OrleanSpaces.Registries;
using OrleanSpaces.Tuples.Specialized;

namespace OrleanSpaces.Agents;

[ImplicitStreamSubscription(Constants.StreamName)]
internal sealed class IntAgent : BaseAgent<int, IntTuple, IntTemplate>
{
    public IntAgent(
        IClusterClient client,
        EvaluationChannel<IntTuple> evaluationChannel,
        ObserverRegistry<IntTuple> observerRegistry,
        CallbackRegistry<int, IntTuple, IntTemplate> callbackRegistry)
        : base(evaluationChannel, observerRegistry, callbackRegistry) { }
}