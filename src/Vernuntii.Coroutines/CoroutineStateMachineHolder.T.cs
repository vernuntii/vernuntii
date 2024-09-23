using System.Diagnostics;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>Provides a strongly-typed box object based on the specific state machine type in use.</summary>
internal sealed class CoroutineStateMachineHolder<TResult, TStateMachine> : CoroutineStateMachineHolder<TResult>, IValueTaskSource<TResult>, IValueTaskSource,
    ICoroutineStateMachineHolder<TResult>, IThreadPoolWorkItem, ICoroutineResultStateMachineHolder, IAsyncIteratorStateMachineHolder<TResult>
    where TStateMachine : IAsyncStateMachine
{
    /// <summary>Delegate used to invoke on an ExecutionContext when passed an instance of this box type.</summary>
    private static readonly ContextCallback s_callback = ExecutionContextCallback;

    /// <summary>Per-core cache of boxes, with one box per core.</summary>
    /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
    private static readonly CacheLineSizePaddedReference[] s_perCoreCache = new CacheLineSizePaddedReference[Environment.ProcessorCount];

    /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
    private static ref CoroutineStateMachineHolder<TResult, TStateMachine>? PerCoreCacheSlot {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers are RentFrom/ReturnToCache
        get {
            // Get the current processor ID.  We need to ensure it fits within s_perCoreCache, so we
            // could % by its length, but we can do so instead by Environment.ProcessorCount, which will be a const
            // in tier 1, allowing better code gen, and then further use uints for even better code gen.
            Debug.Assert(s_perCoreCache.Length == Environment.ProcessorCount, $"{s_perCoreCache.Length} != {Environment.ProcessorCount}");
            var i = (int)((uint)Thread.GetCurrentProcessorId() % (uint)Environment.ProcessorCount);

            // We want an array of StateMachineBox<> objects, each consuming its own cache line so that
            // elements don't cause false sharing with each other.  But we can't use StructLayout.Explicit
            // with generics.  So we use object fields, but always reinterpret them (for all reads and writes
            // to avoid any safety issues) as StateMachineBox<> instances.
#if DEBUG
            object? transientValue = s_perCoreCache[i].Object;
            Debug.Assert(transientValue is null || transientValue is CoroutineStateMachineHolder<TResult, TStateMachine>,
                $"Expected null or {nameof(CoroutineStateMachineHolder<TResult, TStateMachine>)}, got '{transientValue}'");
#endif
            return ref Unsafe.As<object?, CoroutineStateMachineHolder<TResult, TStateMachine>?>(ref s_perCoreCache[i].Object);
        }
    }

    /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
    [ThreadStatic]
    private static CoroutineStateMachineHolder<TResult, TStateMachine>? t_tlsCache;

    /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // only one caller
    internal static CoroutineStateMachineHolder<TResult, TStateMachine> RentFromCache()
    {
        // First try to get a box from the per-thread cache.
        var box = t_tlsCache;

        if (box is not null) {
            t_tlsCache = null;
        } else {
            // If we can't, then try to get a box from the per-core cache.
            ref var slot = ref PerCoreCacheSlot;

            if (slot is null || (box = Interlocked.Exchange(ref slot, null)) is null) {
                // If we can't, just create a new one.
                box = new CoroutineStateMachineHolder<TResult, TStateMachine>();
            }
        }

        box.Initialize();
        return box;
    }

    /// <summary>The state machine itself.</summary>
    public TStateMachine? StateMachine;

    /// <summary>A delegate to the <see cref="MoveNext()"/> method.</summary>
    public Action MoveNextAction => _moveNextAction ??= new Action(MoveNext);

    ref CoroutineContext ICoroutineStateMachineHolder.CoroutineContext => ref _coroutineContext;

    private void Initialize()
    {
        _coroutineContext.SetResultStateMachine(this);
        _result = CoroutineStateMachineBoxResult<TResult>.Default;
    }

    /// <summary>Returns this instance to the cache.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers
    private void ReturnToCache()
    {
        // Clear out the state machine and associated context to avoid keeping arbitrary state referenced by
        // lifted locals, and reset the instance for another await.
        StateMachine = default;
        _executionContext = default;
        _coroutineContext.OnCoroutineCompleted();
        _coroutineContext = default;
        //_isCoroutineReserved = 0;
        _valueTaskSource.Reset();

        // If the per-thread cache is empty, store this into it..
        if (t_tlsCache is null) {
            t_tlsCache = this;
        } else {
            // Otherwise, store it into the per-core cache.
            ref var slot = ref PerCoreCacheSlot;

            if (slot is null) {
                // Try to avoid the write if we know the slot isn't empty (we may still have a benign race condition and
                // overwrite what's there if something arrived in the interim).
                Volatile.Write(ref slot, this);
            }
        }
    }

    void ICoroutineResultStateMachineHolder.CallbackWhenForkNotifiedCritically<TAwaiter>(ref TAwaiter forkAwaiter, Action forkCompleted)
    {
        CoroutineStateMachineBoxResult<TResult>? currentState;
        CoroutineStateMachineBoxResult<TResult> newState;

        do {
            currentState = _result;

            if (currentState is null || currentState.State != CoroutineStateMachineBoxResult<TResult>.ResultState.NotYetComputed) {
                throw new InvalidOperationException("Result state machine has already finished");
            }

            newState = new CoroutineStateMachineBoxResult<TResult>(currentState.ForkCount + 1);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

        forkAwaiter.UnsafeOnCompleted(() => {
            Exception? childError = null;

            try {
                forkCompleted();
            } catch (Exception error) {
                childError = error;
            }

            CoroutineStateMachineBoxResult<TResult>? currentState;
            CoroutineStateMachineBoxResult<TResult>? newState;

            do {
                currentState = _result;

                if (currentState is null) {
                    return;
                }

                if (currentState.ForkCount == 1 && currentState.State != CoroutineStateMachineBoxResult<TResult>.ResultState.NotYetComputed) {
                    newState = null;
                } else {
                    newState = new CoroutineStateMachineBoxResult<TResult>(currentState, currentState.ForkCount - 1);
                }
            } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

            if (newState is null) {
                if (currentState.HasResult) {
                    SetResultCore(currentState.Result);
                } else if (currentState.HasError) {
                    SetExceptionCore(currentState.Error);
                } else if (childError is not null) {
                    SetExceptionCore(childError);
                }
            }
        });
    }

    /// <summary>
    /// Used to initialize s_callback above. We don't use a lambda for this on purpose: a lambda would
    /// introduce a new generic type behind the scenes that comes with a hefty size penalty in AOT builds.
    /// </summary>
    private static void ExecutionContextCallback(object? s)
    {
        // Only used privately to pass directly to EC.Run
        Debug.Assert(s is CoroutineStateMachineHolder<TResult, TStateMachine>, $"Expected {nameof(CoroutineStateMachineHolder<TResult, TStateMachine>)}, got '{s}'");
        Unsafe.As<CoroutineStateMachineHolder<TResult, TStateMachine>>(s).StateMachine!.MoveNext();
    }

    /// <summary>Calls MoveNext on <see cref="StateMachine"/></summary>
    public void MoveNext()
    {
        var context = _executionContext;

        if (context is null) {
            Debug.Assert(StateMachine is not null, $"Null {nameof(StateMachine)}");
            StateMachine.MoveNext();
        } else {
            ExecutionContext.Run(context, s_callback, this);
        }
    }

    /// <summary>Invoked to run MoveNext when this instance is executed from the thread pool.</summary>
    void IThreadPoolWorkItem.Execute() => MoveNext();

    /// <summary>Get the result of the operation.</summary>
    TResult IValueTaskSource<TResult>.GetResult(short token)
    {
        try {
            return _valueTaskSource.GetResult(token);
        } finally {
            ReturnToCache();
        }
    }

    /// <summary>Get the result of the operation.</summary>
    void IValueTaskSource.GetResult(short token)
    {
        try {
            _valueTaskSource.GetResult(token);
        } finally {
            ReturnToCache();
        }
    }

    void IAsyncIteratorStateMachineHolder<TResult>.SetAsyncIteratorCompletionSource(IValueTaskCompletionSource<TResult>? completionSource) =>
        _valueTaskSource._asyncIteratorCompletionSource = completionSource;

    void IAsyncIteratorStateMachineHolder<TResult>.SetResult(TResult result) => _valueTaskSource._valueTaskSource.SetResult(result);

    void IAsyncIteratorStateMachineHolder<TResult>.SetException(Exception e) => _valueTaskSource._valueTaskSource.SetException(e);

    IAsyncIteratorStateMachineHolder<Nothing> IAsyncIteratorStateMachineHolder.CreateNewByCloningUnderlyingStateMachine(
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint)
    {
        Debug.Assert(StateMachine is not null);
        var theirStateMachine = CoroutineStateMachineAccessor<TStateMachine>.CloneStateMachine(StateMachine);
        ref var theirBuilder = ref CoroutineStateMachineAccessor<TStateMachine>.GetCoroutineMethodBuilder(ref theirStateMachine);
        var theirStateMachineHolder = theirBuilder.ReplaceCoroutineUnderlyingStateMachine(ref theirStateMachine);
        ourSuspensionPoint._coroutineAwaiter.RenewStateMachineCoroutineAwaiter<TStateMachine>(theirStateMachineHolder, in ourSuspensionPoint, ref theirSuspensionPoint);
        return theirBuilder.ReplaceCoroutineUnderlyingStateMachine(ref theirStateMachine);
    }

    IAsyncIteratorStateMachineHolder<TResult> IAsyncIteratorStateMachineHolder<TResult>.CreateNewByCloningUnderlyingStateMachine(
        in SuspensionPoint ourSuspensionPoint,
        ref SuspensionPoint theirSuspensionPoint)
    {
        Debug.Assert(StateMachine is not null);
        var theirStateMachine = CoroutineStateMachineAccessor<TStateMachine, TResult>.CloneStateMachine(in StateMachine);
        ref var theirBuilder = ref CoroutineStateMachineAccessor<TStateMachine, TResult>.GetCoroutineMethodBuilder(ref theirStateMachine);
        var theirStateMachineHolder = theirBuilder.ReplaceCoroutineUnderlyingStateMachine(ref theirStateMachine);
        ourSuspensionPoint._coroutineAwaiter.RenewStateMachineCoroutineAwaiter<TStateMachine>(theirStateMachineHolder, in ourSuspensionPoint, ref theirSuspensionPoint);
        return theirStateMachineHolder;
    }
}
