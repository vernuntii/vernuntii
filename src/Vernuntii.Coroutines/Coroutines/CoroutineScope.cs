using System.Text;

namespace Vernuntii.Coroutines;

public sealed class CoroutineScope : IDisposable
{
    internal static readonly Key s_coroutineScopeKey = new(Encoding.ASCII.GetBytes(nameof(CoroutineScope)), isContextService: true, inheritable: true);

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
