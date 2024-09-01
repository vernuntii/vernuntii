namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIteratorStateMachineBox<TResult> : ICoroutineStateMachineBox
{
    void SetAsyncIteratorCompletionSource(IAsyncIteratorCompletionSource<TResult>? completionSource);
    void SetResult(TResult result);
    void SetException(Exception e);
}
