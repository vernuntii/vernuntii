using System.Text;

namespace Vernuntii.Coroutines.Iterators;

partial class AsyncIteratorImpl<TResult>
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    object IAsyncIterator<TResult>.Current => Current;

    ValueTask<bool> IAsyncIterator<TResult>.MoveNextAsync() => MoveNextAsync();

    void IAsyncIterator<TResult>.YieldReturn<TYieldResult>(TYieldResult result) => YieldReturn(result);

    void IAsyncIterator<TResult>.Return(TResult result) => Return(result);

    void IAsyncIterator<TResult>.Throw(Exception e) => Throw(e);

    TResult IAsyncIterator<TResult>.GetResult() => GetResult();

    Coroutine<TResult> IAsyncIterator<TResult>.GetResultAsync() => GetResultAsync();
}
