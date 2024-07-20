namespace Vernuntii.Reactive.Coroutines.Stepping;

public class TraceStepCompletionAwaiter<T> : AbstractStepCompletionAwaiter<EventTrace<T>>
{
    private readonly EventTrace<T> _trace;

    internal TraceStepCompletionAwaiter(IStep step, EventTrace<T> trace)
        : base(step)
    {
        _trace = trace;
    }

    protected override void SetResult(TaskCompletionSource<EventTrace<T>> taskSource) =>
        taskSource.SetResult(_trace);

    public override EventTrace<T> GetResult() => Result;
}
