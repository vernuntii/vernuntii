namespace Vernuntii.Coroutines;

internal interface ICoroutineResultStateMachineBox
{
    void CallbackWhenForkCompletedUnsafely<TAwaiter>(ref TAwaiter forkAwaiter, Action forkCompleted) where TAwaiter : ICriticalNotifyCompletion;
}
