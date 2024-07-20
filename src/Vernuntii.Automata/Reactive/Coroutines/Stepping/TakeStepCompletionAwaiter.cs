namespace Vernuntii.Reactive.Coroutines.Stepping;

public sealed class TakeStepCompletionAwaiter<T> : AbstractStepCompletionAwaiter<T>
{
    private readonly YieldResult<T> _emission;

    internal TakeStepCompletionAwaiter(IStep step, YieldResult<T> emission)
        : base(step)
    {
        _emission = emission;
    }

    protected override void SetResult(TaskCompletionSource<T> taskSource) =>
        taskSource.SetResult(_emission.Value);

    public override T GetResult() => Result;
}
