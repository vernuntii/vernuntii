using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal interface ICoroutineResultStateMachineBox
{
    void CallbackWhenForkCompletedUnsafe<TAwaiter>(ref TAwaiter awaiter, Action continuation) where TAwaiter : ICriticalNotifyCompletion;
}
