using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal interface ICoroutineResultStateMachine
{
    void AwaitUnsafeOnCompleted<TAwaiter>(ref TAwaiter awaiter, Action continuation) where TAwaiter : ICriticalNotifyCompletion;
}
