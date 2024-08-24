namespace Vernuntii.Coroutines;

public sealed class CoroutineScope : IDisposable
{
#if DEBUG
    internal int CoroutineActivationCounter => _coroutineActivationCounter;

    private int _coroutineActivationCounter;
#endif

    public int OnCoroutineStarted()
    {
#if DEBUG
        return Interlocked.Increment(ref _coroutineActivationCounter);
#else
        return 0;
#endif
    }

    public void OnCoroutineCompleted()
    {
#if DEBUG
        Interlocked.Decrement(ref _coroutineActivationCounter);
#endif
    }

    public void Dispose() => throw new NotImplementedException();
}
