using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public class AsyncIterator<TResult> : IAsyncIterator<TResult>
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    private AsyncIteratorCore<TResult> _asyncIterator;

    public object Current => _asyncIterator.Current;

    public AsyncIterator(Func<Coroutine<TResult>> provider) => _asyncIterator = new AsyncIteratorCore<TResult>(provider);

    public AsyncIterator(Coroutine<TResult> coroutine) => _asyncIterator = new AsyncIteratorCore<TResult>(coroutine);

    public ValueTask<bool> MoveNextAsync() => _asyncIterator.MoveNextAsync();

    public void YieldReturn<TYieldResult>(TYieldResult result) => _asyncIterator.YieldReturn(result);

    public void Return(TResult result) => _asyncIterator.Return(result);

    public void Throw(Exception e) => _asyncIterator.Throw(e);

    public TResult GetResult() => _asyncIterator.GetResult();

    public Coroutine<TResult> GetResultAsync() => _asyncIterator.GetResultAsync();
}
