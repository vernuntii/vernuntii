using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public abstract class AbstractCoroutineArgumentReceiverAcceptor : ISiblingCoroutine
{
    //private int _hasCoroutineBeenActedOn;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver);

    void ISiblingCoroutine.AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver) => AcceptCoroutineArgumentReceiver(ref argumentReceiver);
}
