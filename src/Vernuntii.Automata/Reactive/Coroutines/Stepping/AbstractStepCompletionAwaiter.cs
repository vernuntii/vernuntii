using System.Runtime.CompilerServices;

namespace Vernuntii.Reactive.Coroutines.Stepping;

public abstract class AbstractStepCompletionAwaiter<T> : ICriticalNotifyCompletion, IStepCompletionHandler
{
    public bool IsCompleted => _taskSource?.Task.IsCompleted ?? false;
    public IStep Step { get; }

    /// <summary>
    /// Lazily loaded. Use it with extreme caution. Use it only for <see cref="GetResult"/>.
    /// </summary>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>")]
    protected T Result => GetTaskSourceOrInitialize().Task.Result;

    private TaskCompletionSource<T>? _taskSource;

    protected AbstractStepCompletionAwaiter(IStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        Step = step;
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

    void IStepCompletionHandler.CompleteStep() => SetResult(GetTaskSourceOrInitialize());
}
