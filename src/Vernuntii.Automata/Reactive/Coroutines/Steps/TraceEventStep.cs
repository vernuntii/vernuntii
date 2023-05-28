namespace Vernuntii.Reactive.Coroutines.Steps;

internal class TraceEventStep<T> : ITraceEventStep
{
    public StepHandlerId HandlerId => TraceEventStepHandler.HandlerId;
    public EventTrace<T> Trace { get; }
    public EventConnector<T> Connector { get; }

    IEventTrace ITraceEventStep.Trace => Trace;
    IEventConnector ITraceEventStep.Connector => Connector;

    public TraceEventStep(EventTrace<T> trace, EventConnector<T> connector)
    {
        Trace = trace;
        Connector = connector;
    }
}
