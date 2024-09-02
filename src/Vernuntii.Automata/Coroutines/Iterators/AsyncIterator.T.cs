using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public class AsyncIterator<TReturnResult> : IAsyncIterator<TReturnResult>
{
    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    private AsyncIteratorCore<TReturnResult> _asyncIterator;

    public object Current => _asyncIterator.Current;

    public AsyncIterator(Func<Coroutine<TReturnResult>> provider)
    {
        _asyncIterator = new AsyncIteratorCore<TReturnResult>(provider);
    }

    public AsyncIterator(Coroutine<TReturnResult> coroutine)
    {
        _asyncIterator = new AsyncIteratorCore<TReturnResult>(coroutine);
    }

    public ValueTask<bool> MoveNextAsync() => _asyncIterator.MoveNextAsync();

    public void YieldReturn<TResult>(TResult result) => _asyncIterator.YieldReturn(result);

    public void Return(TReturnResult result) => _asyncIterator.Return(result);

    public void Throw(Exception e) => _asyncIterator.Throw(e);

    public TReturnResult GetResult() => _asyncIterator.GetResult();

    public Coroutine<TReturnResult> GetResultAsync() => _asyncIterator.GetResultAsync();
}
