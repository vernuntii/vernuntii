using System.Collections.Concurrent;

namespace Vernuntii.Reactive.Coroutines.Steps;

internal class TraceEventStepHandler : IStepHandler
{
    public readonly static StepHandlerId HandlerId = new StepHandlerId(typeof(TraceEventStepHandler));

    private ConcurrentDictionary<int, IEventConnection> _connections = new();
    private int _traceCounter;

    public ValueTask HandleAsync(IStep step)
    {
        if (step is not ITraceEventStep traceStep) {
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
