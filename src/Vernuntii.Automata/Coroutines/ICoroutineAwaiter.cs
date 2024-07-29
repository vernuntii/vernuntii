namespace Vernuntii.Coroutines;

public interface ICoroutineAwaiter {
    internal bool IsChildCoroutine { get; }

    CoroutineArgumentReceiverAcceptor? ArgumentReceiverAcceptor { get; }
}
