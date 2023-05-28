namespace Vernuntii.Reactive.Coroutines.Steps;

interface ITakeEventStep : IStep
{
    IEventTrace Trace { get; }

    Task HandleAsync(IEventConnection connection, CancellationToken cancellationToken);
}
