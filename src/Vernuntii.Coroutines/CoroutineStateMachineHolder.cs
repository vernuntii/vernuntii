using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>The base type for all value task box reusable box objects, regardless of state machine type.</summary>
internal abstract class CoroutineStateMachineHolder<TResult> : IValueTaskSource<TResult>, IValueTaskSource, ICoroutineResultStateMachineHolder, IChildCoroutine
{
    internal readonly static CoroutineStateMachineHolder<TResult> s_synchronousSuccessSentinel = new SynchronousSuccessSentinelCoroutineStateMachineBox();

    /// <summary>Gets the current version number of the box.</summary>
    public short Version => _valueTaskSource.Version;

    /// <summary>A delegate to the MoveNext method.</summary>
    protected Action? _moveNextAction;

    /// <summary>Captured ExecutionContext with which to invoke MoveNext.</summary>
    internal ExecutionContext? _executionContext;

    internal CoroutineContext _coroutineContext;

    /// <summary>Implementation for IValueTaskSource interfaces.</summary>
    internal protected ManualResetValueTaskSourceProxy<TResult> _valueTaskSource;

    protected CoroutineStateMachineBoxResult<TResult>? _result;

    protected CoroutineStateMachineHolder()
    {
        _coroutineContext._bequesterOrigin = CoroutineContextBequesterOrigin.ChildCoroutine;
    }

    void IChildCoroutine.ActOnCoroutine(in CoroutineContext contextToBequest)
    {
        CoroutineContext.InheritOrBequestCoroutineContext(ref _coroutineContext, in contextToBequest);
        ref var coroutineContext = ref _coroutineContext;
        coroutineContext.OnCoroutineStarted();
        Unsafe.As<ICoroutineStateMachineHolder>(this).MoveNext();
    }

    void ICoroutineResultStateMachineHolder.CallbackWhenForkNotifiedCritically<TAwaiter>(ref TAwaiter awaiter, Action continuation) =>
        throw Exceptions.ImplementedByDerivedType();

    /// <summary>Gets the status of the box.</summary>
    public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

    /// <summary>Schedules the continuation action for this box.</summary>
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _valueTaskSource.OnCompleted(continuation, state, token, flags);

    protected void SetExceptionCore(Exception error)
    {
        _valueTaskSource.SetException(error);
    }

    protected void SetResultCore(TResult result)
    {
        _valueTaskSource.SetResult(result);
    }

    /// <summary>Completes the box with a result.</summary>
    /// <param name="result">The result.</param>
    public void SetResult(TResult result)
    {
        CoroutineStateMachineBoxResult<TResult> currentState;
        CoroutineStateMachineBoxResult<TResult>? newState;

        do {
            currentState = _result!; // Cannot be null at this state
            newState = currentState.ForkCount == 0 ? null : new CoroutineStateMachineBoxResult<TResult>(currentState.ForkCount, result);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

        if (newState is null) {
            SetResultCore(result);
        }
    }

    /// <summary>Completes the box with an error.</summary>
    /// <param name="error">The exception.</param>
    public void SetException(Exception error)
    {
        CoroutineStateMachineBoxResult<TResult> currentState;
        CoroutineStateMachineBoxResult<TResult>? newState;

        do {
            currentState = _result!; // Cannot be null at this state
            newState = currentState.ForkCount == 0 ? null : new CoroutineStateMachineBoxResult<TResult>(currentState.ForkCount, error);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

        if (newState is null) {
            SetExceptionCore(error);
        }
    }

    /// <summary>Implemented by derived type.</summary>
    TResult IValueTaskSource<TResult>.GetResult(short token) => throw Exceptions.ImplementedByDerivedType();

    /// <summary>Implemented by derived type.</summary>
    void IValueTaskSource.GetResult(short token) => throw Exceptions.ImplementedByDerivedType();

    private static class Exceptions
    {
        public static NotImplementedException ImplementedByDerivedType([CallerMemberName] string? methodName = null)
        {
            return new NotImplementedException($"The method {methodName} must be explicitly overriden by derived type");
        }
    }

    /// <summary>Type used as a singleton to indicate synchronous success for an async method.</summary>
    internal sealed class SynchronousSuccessSentinelCoroutineStateMachineBox : CoroutineStateMachineHolder<TResult>, ICoroutineResultStateMachineHolder
    {
        public SynchronousSuccessSentinelCoroutineStateMachineBox() => SetResultCore(default!);

        void ICoroutineResultStateMachineHolder.CallbackWhenForkNotifiedCritically<TAwaiter>(ref TAwaiter awaiter, Action continuation) =>
            awaiter.UnsafeOnCompleted(continuation);
    }
}
