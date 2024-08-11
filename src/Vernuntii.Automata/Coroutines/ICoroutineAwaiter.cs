namespace Vernuntii.Coroutines;

public interface ICoroutineAwaiter {
    internal bool IsChildCoroutine { get; }
    internal bool IsGenericCoroutine { get; }
    CoroutineArgumentReceiverDelegate? ArgumentReceiverDelegate { get; }
}
