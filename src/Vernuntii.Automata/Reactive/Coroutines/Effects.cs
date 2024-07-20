using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines;

internal static class Effects
{
    public static IStepAwaiterProvider<TraceStepCompletionAwaiter<T>> Trace<T>(IObservableEvent<T> observableEvent)
    {
        var eventTrace = new EventTrace<T>();
        var eventConnector = new EventConnector<T>(eventTrace, observableEvent);
        return new TraceStep<T>(eventTrace, eventConnector);
    }

    /// <summary>
    /// As soon as yielded, the coroutine will wait for the next emission traced by <paramref name="trace"/>.
    /// The action
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="trace"></param>
    /// <param name="emission"></param>
    /// <returns></returns>
    public static IStepAwaiterProvider<TakeStepCompletionAwaiter<T>> Take<T>(EventTrace<T> trace)
    {
        var emission = new YieldResult<T>();
        return new TakeStep<T>(trace, emission);
    }
}
