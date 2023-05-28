using System.Threading.Channels;
using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines;

internal class EventConnection<T> : IEventConnection
{
    public IEventTrace Trace => _trace;

    private readonly EventTrace<T> _trace;
    private IDisposable _subscription;
    private Channel<T> _emissions;

    public EventConnection(EventTrace<T> trace, IObservableEvent<T> observableEvent)
    {
        _emissions = Channel.CreateUnbounded<T>();
        _trace = trace;
        _subscription = observableEvent.Subscribe(async emission => await _emissions.Writer.WriteAsync(emission));
    }

    public async Task<T> GetNextEmissionAsync(CancellationToken cancellationToken) =>
        await _emissions.Reader.ReadAsync(cancellationToken);

    public void Dispose() => _subscription.Dispose();
}
