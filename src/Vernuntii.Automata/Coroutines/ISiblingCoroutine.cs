namespace Vernuntii.Coroutines;

internal interface ISiblingCoroutine
{
    bool IsSiblingCoroutine { get; }

    void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver);
}
