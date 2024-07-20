namespace Vernuntii.Reactive.Coroutines.Stepping;

internal class TraceStep<T> : ITraceEventStep, IStepAwaiterProvider<TraceStepCompletionAwaiter<T>>
{
    public StepHandlerId HandlerId => TraceStepHandler.HandlerId;
    public EventTrace<T> Trace { get; }
    public EventConnector<T> Connector { get; }

    IEventTrace ITraceEventStep.Trace => Trace;
    IEventConnector ITraceEventStep.Connector => Connector;

    public TraceStep(EventTrace<T> trace, EventConnector<T> connector)
    {
        Trace = trace;
        Connector = connector;
    }

    public TraceStepCompletionAwaiter<T> GetAwaiter() => new TraceStepCompletionAwaiter<T>(this, Trace);
}
