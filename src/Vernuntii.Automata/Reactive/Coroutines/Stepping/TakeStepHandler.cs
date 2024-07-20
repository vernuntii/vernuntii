namespace Vernuntii.Reactive.Coroutines.Stepping;

internal class TakeStepHandler : IStepHandler
{
    public readonly static StepHandlerId HandlerId = new StepHandlerId(typeof(TakeStepHandler));

    private readonly TraceStepHandler _handler;

    public TakeStepHandler(TraceStepHandler handler) =>
        _handler = handler;

    public async ValueTask HandleAsync(IStep step)
    {
        if (step is not ITakeStep takeStep) {
            throw new InvalidOperationException();
        }

        var connection = _handler.GetConnection(takeStep.Trace);
        await takeStep.HandleAsync(connection, CancellationToken.None);
    }
}
