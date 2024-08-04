namespace Vernuntii.Coroutines;

public interface ICoroutineAwaiter {
    internal bool IsChildCoroutine { get; }

    CoroutineArgumentReceiverDelegate? ArgumentReceiverDelegate { get; }
}
