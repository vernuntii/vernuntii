namespace Vernuntii.Coroutines;

internal static class CoroutinesMarshal
{
    internal static ICoroutineStateMachineBox GetStateMachaineBox(Coroutine coroutine) =>
        coroutine._builder as ICoroutineStateMachineBox ?? throw new InvalidOperationException();

    internal static ICoroutineStateMachineBox GetStateMachaineBox<TResult>(Coroutine<TResult> coroutine) =>
        coroutine._builder as ICoroutineStateMachineBox ?? throw new InvalidOperationException();
}
