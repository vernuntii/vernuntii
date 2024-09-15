namespace Vernuntii.Coroutines;

internal static class CoroutineAwaiterExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCompletion(this Task task, IValueTaskCompletionSource<Nothing> completionSource)
    {
        var taskAwaiter = task.ConfigureAwait(false).GetAwaiter();

        if (task.IsCompleted) {
            CompleteSource(in taskAwaiter, completionSource);
        } else {
            taskAwaiter.UnsafeOnCompleted(() => CompleteSource(in taskAwaiter, completionSource));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CompleteSource(in ConfiguredTaskAwaitable.ConfiguredTaskAwaiter taskAwaiter, IValueTaskCompletionSource<Nothing> completionSource)
        {
            try {
                taskAwaiter.GetResult();
                completionSource.SetResult(default);
            } catch (Exception error) {
                completionSource.SetException(error);
                //throw; // Must bubble up
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCompletion<TResult>(this ValueTask<TResult> coroutineAwaiter, IValueTaskCompletionSource<TResult> completionSource)
    {
        var taskAwaiter = coroutineAwaiter.ConfigureAwait(false).GetAwaiter();

        if (coroutineAwaiter.IsCompleted) {
            CompleteSource(in taskAwaiter, completionSource);
        } else {
            taskAwaiter.UnsafeOnCompleted(() => CompleteSource(in taskAwaiter, completionSource));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CompleteSource(in ConfiguredValueTaskAwaitable<TResult>.ConfiguredValueTaskAwaiter taskAwaiter, IValueTaskCompletionSource<TResult> completionSource)
        {
            try {
                var result = taskAwaiter.GetResult();
                completionSource.SetResult(result);
            } catch (Exception error) {
                completionSource.SetException(error);
                //throw; // Must bubble up
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCoroutineCompletion<TAwaiter>(this ref TAwaiter coroutineAwaiter, IValueTaskCompletionSource<Nothing> completionSource)
        where TAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        if (coroutineAwaiter.IsCompleted) {
            CompleteSource(in coroutineAwaiter, completionSource);
        } else {
            var coroutineAwaiterCopy = coroutineAwaiter;
            coroutineAwaiter.UnsafeOnCompleted(() => CompleteSource(in coroutineAwaiterCopy, completionSource));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CompleteSource(in TAwaiter coroutineAwaiter, IValueTaskCompletionSource<Nothing> completionSource)
        {
            try {
                coroutineAwaiter.GetResult();
                completionSource.SetResult(default);
            } catch (Exception error) {
                completionSource.SetException(error);
                //throw; // Must bubble up
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCoroutineCompletion<TAwaiter, TResult>(this ref TAwaiter coroutineAwaiter, IValueTaskCompletionSource<TResult> completionSource)
        where TAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter<TResult>
    {
        if (coroutineAwaiter.IsCompleted) {
            CompleteSource(in coroutineAwaiter, completionSource);
        } else {
            var coroutineAwaiterCopy = coroutineAwaiter;
            coroutineAwaiter.UnsafeOnCompleted(() => CompleteSource(in coroutineAwaiterCopy, completionSource));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CompleteSource(in TAwaiter coroutineAwaiter, IValueTaskCompletionSource<TResult> completionSource)
        {
            try {
                var result = coroutineAwaiter.GetResult();
                completionSource.SetResult(result);
            } catch (Exception error) {
                completionSource.SetException(error);
                //throw; // Must bubble up
            }
        }
    }
}
