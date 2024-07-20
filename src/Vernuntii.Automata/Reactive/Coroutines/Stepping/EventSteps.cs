namespace Vernuntii.Reactive.Coroutines.Stepping;

public static class EventSteps
{
    public static IStep Trace<T>(this ICoroutines _, IObservableEvent<T> observableEvent, out EventTrace<T> eventTrace)
    {
        eventTrace = new EventTrace<T>();
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
    public static IStep Take<T>(this ICoroutines _, EventTrace<T> trace, out IYieldResult<T> emission)
    {
        var typedEmission = new YieldResult<T>();
        emission = typedEmission;
        return new TakeStep<T>(trace, typedEmission);
    }
}
