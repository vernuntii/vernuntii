namespace Vernuntii.Reactive.Coroutines.Steps;

internal class EventStepStore : KeyedStepStore
{
    protected override Dictionary<StepHandlerId, IStepHandler> StepHandlers { get; }

    public EventStepStore()
    {
        var traceEventStepHandler = new TraceEventStepHandler();
        var takeEventStepHandler = new TakeEventStepHandler(traceEventStepHandler);

        StepHandlers = new() {
            {  TraceEventStepHandler.HandlerId, traceEventStepHandler },
            {  TakeEventStepHandler.HandlerId, takeEventStepHandler }
        };
    }
}
