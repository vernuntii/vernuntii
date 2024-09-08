
using System.Threading.Channels;

namespace Vernuntii.Reactive.Extensions.Coroutines;

public class EventChannel<T> : IDisposable
{
    internal readonly Channel<T> _channel;

    internal EventChannel(Channel<T> channel)
    {
        _channel = channel;
    }

    public ValueTask<T> Take(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAsync(cancellationToken);

    public void Dispose() => throw new NotImplementedException();
}
