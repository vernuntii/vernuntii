namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal interface ITraceEffect : IEffect
{
    IEventTrace Trace { get; }
    IEventConnector Connector { get; }
}

public interface ITraceEventStep<T> : IEffect
{
    TraceEffectCompletionAwaiter<T> GetAwaiter();
}
