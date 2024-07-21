namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public class TraceEffectCompletionAwaiter<T> : AbstractEffectCompletionAwaiter<EventTrace<T>>
{
    private readonly EventTrace<T> _trace;

    internal TraceEffectCompletionAwaiter(IEffect step, EventTrace<T> trace)
        : base(step)
    {
        _trace = trace;
    }

    protected override void SetResult(TaskCompletionSource<EventTrace<T>> taskSource) =>
        taskSource.SetResult(_trace);

    public override EventTrace<T> GetResult() => Result;
}
