namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

record TakeEffect<T>(IEventTrace Trace, YieldResult<T> Emission) : ITakeEffect, IEffectCompletionAwaiterProvider<TakeEffectCompletionAwaiter<T>>
{
    public EffectHandlerId HandlerId => TakeEffectHandler.HandlerId;

    public async Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken)
    {
        var typedConnection = (EventConnection<T>)connection;
        Emission.Value = await typedConnection.GetNextEmissionAsync(cancellationToken).ConfigureAwait(false);
    }

    public TakeEffectCompletionAwaiter<T> GetAwaiter() => new TakeEffectCompletionAwaiter<T>(this, Emission);
}
