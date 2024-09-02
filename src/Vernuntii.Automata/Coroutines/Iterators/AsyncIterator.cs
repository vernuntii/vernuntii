using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines.Iterators;

public class AsyncIterator : IAsyncIterator
{
    public static AsyncIterator Create(Func<Coroutine> provider) => new(provider);

    public static AsyncIterator Create(Coroutine coroutine) => new(coroutine);

    public static AsyncIterator<TResult> Create<TResult>(Func<Coroutine<TResult>> provider) => new(provider);

    public static AsyncIterator<TResult> Create<TResult>(Coroutine<TResult> coroutine) => new(coroutine);

    internal static readonly Key s_asyncIteratorKey = new Key(Encoding.ASCII.GetBytes(nameof(AsyncIterator)));

    private AsyncIteratorCore<Nothing> _asyncIterator;

    public object Current => _asyncIterator.Current;

    public AsyncIterator(Func<Coroutine> provider)
    {
        _asyncIterator = new AsyncIteratorCore<Nothing>(provider);
    }

    public AsyncIterator(Coroutine coroutine)
    {
        _asyncIterator = new AsyncIteratorCore<Nothing>(coroutine);
    }

    public ValueTask<bool> MoveNextAsync() => _asyncIterator.MoveNextAsync();

    public void YieldReturn<TResult>(TResult result) => _asyncIterator.YieldReturn(result);

    public void Return() => _asyncIterator.Return(default);

    public void Throw(Exception e) => _asyncIterator.Throw(e);

    public void GetResult() => _ = _asyncIterator.GetResult();

    public Coroutine GetResultAsync()
    {
        var coroutine = _asyncIterator.GetResultAsync();
        return Unsafe.As<Coroutine<Nothing>, Coroutine>(ref coroutine);
    }
}
