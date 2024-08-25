using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.Iterators;

internal static class CoroutineContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool MustSupplyAsyncIterator(this CoroutineContext context) => context.IsAsyncIteratorAware && context.BequesterOrigin == CoroutineContextBequesterOrigin.ChildCoroutine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static AsyncIteratorContextService GetAsyncIteratorContextService(this CoroutineContext context)
    {
        if (context.KeyedServices[AsyncIterator.s_asyncIteratorKey] is not AsyncIteratorContextService asyncIteratorContextService) {
            throw new InvalidOperationException($"The async iterator context service must be of type {typeof(AsyncIteratorContextService)}");
        }

        return asyncIteratorContextService;
    }
}
