namespace Vernuntii.Coroutines;

internal static class ValueTaskCompletionSourceExtensions
{
    public static void SetDefaultResult<TResult>(this ValueTaskCompletionSource<TResult> completionSource) =>
        completionSource.SetResult(default!);
}
