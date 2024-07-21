using System.Collections.Concurrent;

namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal class TraceEffectHandler : IEffectHandler
{
    public readonly static EffectHandlerId HandlerId = new EffectHandlerId(typeof(TraceEffectHandler));

    private ConcurrentDictionary<int, IEventConnection> _connections = new();
    private int _traceCounter;

    public ValueTask HandleAsync(IEffect step)
    {
        if (step is not ITraceEffect traceStep) {
            throw new InvalidOperationException();
        }

        if (traceStep.Trace.HasId) {
            throw new InvalidOperationException();
        }

        var eventTraceId = Interlocked.Increment(ref _traceCounter);
        traceStep.Trace.Id = eventTraceId;
        _ = _connections.TryAdd(eventTraceId, traceStep.Connector.Connect());
        return ValueTask.CompletedTask;
    }

    internal IEventConnection GetConnection(IEventTrace trace)
    {
        if (!_connections.TryGetValue(trace.Id, out var connection)) {
            throw new KeyNotFoundException();
        }

        return connection;
    }
}
