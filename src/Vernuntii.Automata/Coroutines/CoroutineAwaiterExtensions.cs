using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineAwaiterExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCompletion<TAwaiter>(this ref TAwaiter coroutineAwaiter, IValueTaskCompletionSource<Nothing> completionSource)
        where TAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter
    {
        if (coroutineAwaiter.IsCompleted) {
            try {
                coroutineAwaiter.GetResult();
                completionSource.SetResult(default);
            } catch (Exception error) {
                completionSource.SetException(error);
            }
        } else {
            coroutineAwaiter.UnsafeOnCompleted(ActionClosure.Create(coroutineAwaiter, completionSource, static (coroutineAwaiterCopy, completionSource) => {
                try {
                    coroutineAwaiterCopy.GetResult();
                    completionSource.SetResult(default);
                } catch (Exception error) {
                    completionSource.SetException(error);
                }
            }).Delegate);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DelegateCompletion<TAwaiter, TResult>(this ref TAwaiter coroutineAwaiter, IValueTaskCompletionSource<TResult> completionSource)
        where TAwaiter : struct, ICriticalNotifyCompletion, ICoroutineAwaiter<TResult>
    {
        if (coroutineAwaiter.IsCompleted) {
            try {
                var result = coroutineAwaiter.GetResult();
                completionSource.SetResult(result);
            } catch (Exception error) {
                completionSource.SetException(error);
            }
        } else {
            var coroutineAwaiterCopy = coroutineAwaiter;
            coroutineAwaiter.UnsafeOnCompleted(ActionClosure.Create(coroutineAwaiter, completionSource, static (coroutineAwaiterCopy, completionSource) => {
                try {
                    var result = coroutineAwaiterCopy.GetResult();
                    completionSource.SetResult(result);
                } catch (Exception error) {
                    completionSource.SetException(error);
                }
            }).Delegate);
        }
    }
}
