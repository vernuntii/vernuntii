namespace Vernuntii.Coroutines;

internal static class RelativeCoroutineThrowHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InvalidOperationException CannotBePreprocessedTwice() =>
        new InvalidOperationException("Coroutine can only be preprocessed once");
}
