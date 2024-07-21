namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public sealed class TakeEffectCompletionAwaiter<T> : AbstractEffectCompletionAwaiter<T>
{
    private readonly YieldResult<T> _emission;

    internal TakeEffectCompletionAwaiter(IEffect step, YieldResult<T> emission)
        : base(step)
    {
        _emission = emission;
    }

    protected override void SetResult(TaskCompletionSource<T> taskSource) =>
        taskSource.SetResult(_emission.Value);

    public override T GetResult() => Result;
}
