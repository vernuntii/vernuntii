namespace Vernuntii.Coroutines;

internal interface ICoroutineResultStateMachineHolder
{
    void CallbackWhenForkNotifiedCritically<TAwaiter>(ref TAwaiter forkAwaiter, Action forkCompleted) where TAwaiter : ICriticalNotifyCompletion;
}
