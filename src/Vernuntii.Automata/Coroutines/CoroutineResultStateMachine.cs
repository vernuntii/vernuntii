using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineResultStateMachine
{
    internal static readonly ICoroutineResultStateMachine s_immediateContinuingResultStateMachine = new PassThrougingCoroutineResultStateMachine();

    private class PassThrougingCoroutineResultStateMachine : ICoroutineResultStateMachine {
        public void AwaitUnsafeOnCompletedThenContinueWith<TAwaiter>(ref TAwaiter awaiter, Action continuation) where TAwaiter : ICriticalNotifyCompletion {
            awaiter.UnsafeOnCompleted(continuation);
        }
    }
}
