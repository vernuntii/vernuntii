namespace Vernuntii.Coroutines;

internal static class ManualResetCoroutineCompletionSourceExtensions
{
    public static void SetDefaultResult<TResult>(this ManualResetCoroutineCompletionSource<TResult> completionSource) =>
        completionSource.SetResult(default!);
}
