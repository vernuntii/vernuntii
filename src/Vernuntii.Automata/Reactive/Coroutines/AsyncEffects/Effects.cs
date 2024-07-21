namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal static class Effects
{
    public static IEffectCompletionAwaiterProvider<TraceEffectCompletionAwaiter<T>> Trace<T>(IObservableEvent<T> observableEvent)
    {
        var eventTrace = new EventTrace<T>();
        var eventConnector = new EventConnector<T>(eventTrace, observableEvent);
        return new TraceEffect<T>(eventTrace, eventConnector);
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
    public static IEffectCompletionAwaiterProvider<TakeEffectCompletionAwaiter<T>> Take<T>(EventTrace<T> trace)
    {
        var emission = new YieldResult<T>();
        return new TakeEffect<T>(trace, emission);
    }

    public static IEffectCompletionAwaiterProvider<AllEffectCompletionAwaiter<T>> All<T>(object work, T workContext)
    {
        return null!;
    }

    public static IEffectCompletionAwaiterProvider<AllEffectCompletionAwaiter<T>> Call<T>(Func<Task<T>> work)
    {
        return null!;
    }
}
