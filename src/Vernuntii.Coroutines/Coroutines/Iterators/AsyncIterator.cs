using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public static class AsyncIterator
{
    internal static readonly Key s_asyncIteratorKey = new(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    public static IAsyncIterator Create(Func<Coroutine> provider) => new AsyncIteratorImpl<Nothing>(provider);

    public static IAsyncIterator Create(Coroutine coroutine) => new AsyncIteratorImpl<Nothing>(coroutine);

    public static IAsyncIterator<TResult> Create<TResult>(Func<Coroutine<TResult>> provider) => new AsyncIteratorImpl<TResult>(provider);

    public static IAsyncIterator<TResult> Create<TResult>(Coroutine<TResult> coroutine) => new AsyncIteratorImpl<TResult>(coroutine);
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
