using System.Runtime.CompilerServices;

namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public abstract class AbstractEffectCompletionAwaiter<T> : ICriticalNotifyCompletion, IEffectCompletionHandler
{
    public bool IsCompleted => _taskSource?.Task.IsCompleted ?? false;
    public IEffect Effect { get; }

    /// <summary>
    /// Lazily loaded. Use it with extreme caution. Use it only for <see cref="GetResult"/>.
    /// </summary>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
    protected T Result => GetTaskSourceOrInitialize().Task.Result;

    private TaskCompletionSource<T>? _taskSource;

    protected AbstractEffectCompletionAwaiter(IEffect step)
    {
        ArgumentNullException.ThrowIfNull(step);
        Effect = step;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TaskCompletionSource<T> GetTaskSourceOrInitialize() =>
        _taskSource ??= new();

    public void OnCompleted(Action continuation) => throw new NotImplementedException();

    public void UnsafeOnCompleted(Action continuation)
    {
        ArgumentNullException.ThrowIfNull(continuation);

        _ = GetTaskSourceOrInitialize().Task.ContinueWith(_ => {
            continuation();
        }, TaskScheduler.Default);
    }

    protected abstract void SetResult(TaskCompletionSource<T> taskSource);

    public abstract T GetResult();

    void IEffectCompletionHandler.CompleteEffect() => SetResult(GetTaskSourceOrInitialize());
}
