namespace Vernuntii.Reactive.Coroutines.Stepping;

record TakeStep<T>(IEventTrace Trace, YieldResult<T> Emission) : ITakeStep, IStepAwaiterProvider<TakeStepCompletionAwaiter<T>>
{
    public StepHandlerId HandlerId => TakeStepHandler.HandlerId;

    public async Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken)
    {
        var typedConnection = (EventConnection<T>)connection;
        Emission.Value = await typedConnection.GetNextEmissionAsync(cancellationToken).ConfigureAwait(false);
    }

    public TakeStepCompletionAwaiter<T> GetAwaiter() => new TakeStepCompletionAwaiter<T>(this, Emission);
}
