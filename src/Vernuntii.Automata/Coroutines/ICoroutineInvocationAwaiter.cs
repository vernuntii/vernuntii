namespace Vernuntii.Coroutines;

public interface ICoroutineInvocationAwaiter {
    internal bool IsChildCoroutine => false;
}
