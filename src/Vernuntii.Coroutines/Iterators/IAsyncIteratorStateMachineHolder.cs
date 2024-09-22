using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIteratorStateMachineHolder : ICoroutineStateMachineHolder, IValueTaskSource
{
    short Version { get; }

    IAsyncIteratorStateMachineHolder<Nothing> CreateNewByCloningUnderlyingStateMachine(in SuspensionPoint ourSuspensionPoint, ref SuspensionPoint theirSuspensionPoint);
}

internal interface IAsyncIteratorStateMachineHolder<TResult> : IAsyncIteratorStateMachineHolder, ICoroutineStateMachineHolder<TResult>, IValueTaskSource<TResult>
{
    short Version { get; }

    void SetAsyncIteratorCompletionSource(IValueTaskCompletionSource<TResult>? completionSource);
    void SetResult(TResult result);
    void SetException(Exception e);

    new IAsyncIteratorStateMachineHolder<TResult> CreateNewByCloningUnderlyingStateMachine(in SuspensionPoint ourSuspensionPoint, ref SuspensionPoint theirSuspensionPoint);
}
