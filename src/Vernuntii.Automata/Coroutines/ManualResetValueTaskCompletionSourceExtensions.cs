namespace Vernuntii.Coroutines;

internal static class ManualResetValueTaskCompletionSourceExtensions
{
    public static void SetDefaultResult<TResult>(this ManualResetValueTaskCompletionSource<TResult> completionSource) =>
        completionSource.SetResult(default!);
}
