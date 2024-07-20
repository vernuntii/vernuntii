namespace Vernuntii.Reactive.Coroutines.Stepping;

interface ITakeStep : IStep
{
    IEventTrace Trace { get; }

    Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken);
}

public interface ITakeEventStep<T> : IStep
{
    TraceStepCompletionAwaiter<T> GetAwaiter();
}
