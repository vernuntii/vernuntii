using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncIterator Create(Func<Coroutine> provider, in CoroutineContext additiveContext = default) => new AsyncIteratorImpl<Nothing>(provider, in additiveContext);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncIterator Create(Coroutine coroutine, in CoroutineContext additiveContext = default) => new AsyncIteratorImpl<Nothing>(coroutine, in additiveContext);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncIterator<TResult> Create<TResult>(Func<Coroutine<TResult>> provider, in CoroutineContext additiveContext = default) => new AsyncIteratorImpl<TResult>(provider, in additiveContext);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncIterator<TResult> Create<TResult>(Coroutine<TResult> coroutine, in CoroutineContext additiveContext = default) => new AsyncIteratorImpl<TResult>(coroutine, in additiveContext);
}

partial class AsyncIteratorImpl<TReturnResult>
{
    object IAsyncIterator.Current => Current;

    ValueTask<bool> IAsyncIterator.MoveNextAsync() => MoveNextAsync();

    void IAsyncIterator.YieldReturn<TYieldResult>(TYieldResult result) => YieldReturn(result);

    void IAsyncIterator.Return() => Return(default!);

    void IAsyncIterator.Throw(Exception e) => Throw(e);

    void IAsyncIterator.GetResult() => _ = GetResult();

    Coroutine IAsyncIterator.GetResultAsync()
    {
        var coroutine = GetResultAsync();
        return Unsafe.As<Coroutine<TReturnResult>, Coroutine>(ref coroutine);
    }
}
