namespace Vernuntii.Reactive.Coroutines.Steps;

record TakeEventStep<T>(IEventTrace Trace, YieldResult<T> Emission) : ITakeEventStep
{
    public StepHandlerId HandlerId => TakeEventStepHandler.HandlerId;

    public async Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken)
    {
        var typedConnection = (EventConnection<T>)connection;
        Emission.Value = await typedConnection.GetNextEmissionAsync(cancellationToken);
    }
}
