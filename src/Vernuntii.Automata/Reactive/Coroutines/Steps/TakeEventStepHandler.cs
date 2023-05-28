namespace Vernuntii.Reactive.Coroutines.Steps;

internal class TakeEventStepHandler : IStepHandler
{
    public readonly static StepHandlerId HandlerId = new StepHandlerId(typeof(TakeEventStepHandler));

    private readonly TraceEventStepHandler _traceEventStepHandler;

    public TakeEventStepHandler(TraceEventStepHandler traceEventStepHandler) =>
        _traceEventStepHandler = traceEventStepHandler;

    public async ValueTask HandleAsync(IStep step)
    {
        if (step is not ITakeEventStep takeEventStep) {
            throw new InvalidOperationException();
        }

        var connection = _traceEventStepHandler.GetConnection(takeEventStep.Trace);
        await takeEventStep.HandleAsync(connection, CancellationToken.None);
    }
}
