namespace Vernuntii.Coroutines.Iterators;

internal static class CoroutineContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static AsyncIteratorContextService GetAsyncIteratorContextService(this CoroutineContext context) =>
        Unsafe.As<AsyncIteratorContextService>(context.KeyedServices[AsyncIterator.s_asyncIteratorKey]);
}
