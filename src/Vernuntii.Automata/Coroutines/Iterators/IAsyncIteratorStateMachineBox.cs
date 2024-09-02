namespace Vernuntii.Coroutines.Iterators;

internal interface IAsyncIteratorStateMachineBox<TResult> : ICoroutineStateMachineBox
{
    ref CoroutineContext CoroutineContext { get; }
    void SetAsyncIteratorCompletionSource(IValueTaskCompletionSource<TResult>? completionSource);
    void SetResult(TResult result);
    void SetException(Exception e);
}
