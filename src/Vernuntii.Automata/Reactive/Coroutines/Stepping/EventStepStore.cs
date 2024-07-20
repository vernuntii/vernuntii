namespace Vernuntii.Reactive.Coroutines.Stepping;

internal class EventStepStore : KeyedStepStore
{
    protected override Dictionary<StepHandlerId, IStepHandler> StepHandlers { get; }

    public EventStepStore()
    {
        var traceEventStepHandler = new TraceStepHandler();
        var takeEventStepHandler = new TakeStepHandler(traceEventStepHandler);

        StepHandlers = new() {
            {  TraceStepHandler.HandlerId, traceEventStepHandler },
            {  TakeStepHandler.HandlerId, takeEventStepHandler }
        };
    }
}
