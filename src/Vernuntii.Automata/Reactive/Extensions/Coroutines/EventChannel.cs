
namespace Vernuntii.Reactive.Extensions.Coroutines;

public class EventChannel<T> : IEventChannel<T>
{
    private readonly IEventChannel<T> _impl;

    public EventChannel(IEventChannel<T> impl) => _impl = impl;

    public void Dispose() => _impl.Dispose();
}
