namespace Vernuntii.Reactive.Coroutines.Stepping;

internal interface ITraceEventStep : IStep
{
    IEventTrace Trace { get; }
    IEventConnector Connector { get; }
}

public interface ITraceEventStep<T> : IStep
{
    TraceStepCompletionAwaiter<T> GetAwaiter();
}
