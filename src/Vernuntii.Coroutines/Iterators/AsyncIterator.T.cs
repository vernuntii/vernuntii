using System.Text;

namespace Vernuntii.Coroutines.Iterators;

partial class AsyncIteratorImpl<TReturnResult>
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    object IAsyncIterator<TReturnResult>.Current => Current;

    ValueTask<bool> IAsyncIterator<TReturnResult>.MoveNextAsync() => MoveNextAsync();

    void IAsyncIterator<TReturnResult>.Yield<TYieldResult>(TYieldResult result) => YieldReturn(result);

    void IAsyncIterator<TReturnResult>.Return(TReturnResult result) => Return(result);

    void IAsyncIterator<TReturnResult>.Throw(Exception e) => Throw(e);

    TReturnResult IAsyncIterator<TReturnResult>.GetResult() => GetResult();

    Coroutine<TReturnResult> IAsyncIterator<TReturnResult>.GetResultAsync() => GetResultAsync();
}
