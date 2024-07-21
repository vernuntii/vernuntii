namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal class TraceEffect<T> : ITraceEffect, IEffectCompletionAwaiterProvider<TraceEffectCompletionAwaiter<T>>
{
    public EffectHandlerId HandlerId => TraceEffectHandler.HandlerId;
    public EventTrace<T> Trace { get; }
    public EventConnector<T> Connector { get; }

    IEventTrace ITraceEffect.Trace => Trace;
    IEventConnector ITraceEffect.Connector => Connector;

    public TraceEffect(EventTrace<T> trace, EventConnector<T> connector)
    {
        Trace = trace;
        Connector = connector;
    }

    public TraceEffectCompletionAwaiter<T> GetAwaiter() => new TraceEffectCompletionAwaiter<T>(this, Trace);
}
