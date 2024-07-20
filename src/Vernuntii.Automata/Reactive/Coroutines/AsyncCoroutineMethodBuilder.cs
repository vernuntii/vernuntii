using System.Runtime.CompilerServices;

namespace Vernuntii.Reactive.Coroutines;

public class AsyncCoroutineMethodBuilder
{
    public static AsyncCoroutineMethodBuilder Create()
    {
        return new AsyncCoroutineMethodBuilder();
    }

    public Task Task => _source.Task;

    private readonly TaskCompletionSource _source = new();

    public void SetException(Exception e) => _source.SetException(e);

    public void SetResult()
    {
        _source.SetResult();
    }

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }
}
