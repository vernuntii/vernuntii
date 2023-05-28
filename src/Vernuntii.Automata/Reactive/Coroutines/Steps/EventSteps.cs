namespace Vernuntii.Reactive.Coroutines.Steps;

public static class EventSteps
{
    public static IStep Trace<T>(this ICoroutineDefinition _, IObservableEvent<T> observableEvent, out EventTrace<T> eventTrace)
    {
        eventTrace = new EventTrace<T>();
        var eventConnector = new EventConnector<T>(eventTrace, observableEvent);
        return new TraceEventStep<T>(eventTrace, eventConnector);
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
    public static IStep Take<T>(this ICoroutineDefinition _, EventTrace<T> trace, out IYieldResult<T> emission)
    {
        var typedEmission = new YieldResult<T>();
        emission = typedEmission;
        return new TakeEventStep<T>(trace, typedEmission);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="trace"></param>
    /// <param name="emission"></param>
    /// <returns></returns>
    public static IStep TakeSync<T>(this ICoroutineDefinition _, EventTrace<T> trace, out IYieldResult<T> emission)
    {
        var typedEmission = new YieldResult<T>();
        emission = typedEmission;
        return new TakeEventStep<T>(trace, typedEmission);
    }
}
