namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

interface ITakeEffect : IEffect
{
    IEventTrace Trace { get; }

    Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken);
}

public interface ITakeEventStep<T> : IEffect
{
    TraceEffectCompletionAwaiter<T> GetAwaiter();
}
