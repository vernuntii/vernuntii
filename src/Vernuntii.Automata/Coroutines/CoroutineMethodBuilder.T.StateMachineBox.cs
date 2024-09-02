using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

partial struct CoroutineMethodBuilder<TResult>
{
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    /// <summary>The base type for all value task box reusable box objects, regardless of state machine type.</summary>
    internal abstract class CoroutineStateMachineBox : IValueTaskSource<TResult>, IValueTaskSource, ICoroutineResultStateMachineBox, ICoroutineMethodBuilderBox
    {
        internal readonly static CoroutineStateMachineBox s_synchronousSuccessSentinel = new SynchronousSuccessSentinelCoroutineStateMachineBox();

        /// <summary>A delegate to the MoveNext method.</summary>
        protected Action? _moveNextAction;

        /// <summary>Captured ExecutionContext with which to invoke MoveNext.</summary>
        internal ExecutionContext? _executionContext;

        internal CoroutineContext _coroutineContext;

        /// <summary>Implementation for IValueTaskSource interfaces.</summary>
        internal protected ManualResetValueTaskSourceProxy<TResult> _valueTaskSource;

        protected CoroutineStateMachineBoxResult? _result;

        protected CoroutineStateMachineBox()
        {
            _coroutineContext._bequesterOrigin = CoroutineContextBequesterOrigin.ChildCoroutine;
        }

        void IChildCoroutine.InheritCoroutineContext(in CoroutineContext contextToBequest)
        {
            CoroutineContext.InheritOrBequestCoroutineContext(ref _coroutineContext, in contextToBequest);
        }

        void IChildCoroutine.StartCoroutine()
        {
            ref var coroutineContext = ref _coroutineContext;
            coroutineContext.OnCoroutineStarted();
            Unsafe.As<ICoroutineStateMachineBox>(this).MoveNext();
        }

        void ICoroutineResultStateMachineBox.CallbackWhenForkCompletedUnsafely<TAwaiter>(ref TAwaiter awaiter, Action continuation) =>
            throw Exceptions.ImplementedByDerivedType();

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
            CoroutineStateMachineBoxResult currentState;
            CoroutineStateMachineBoxResult? newState;

            do {
                currentState = _result!; // Cannot be null at this state
                newState = currentState.ForkCount == 0 ? null : new CoroutineStateMachineBoxResult(currentState.ForkCount, result);
            } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

            if (newState is null) {
                SetResultCore(result);
            }
        }

        /// <summary>Completes the box with an error.</summary>
        /// <param name="error">The exception.</param>
        public void SetException(Exception error)
        {
            CoroutineStateMachineBoxResult currentState;
            CoroutineStateMachineBoxResult? newState;

            do {
                currentState = _result!; // Cannot be null at this state
                newState = currentState.ForkCount == 0 ? null : new CoroutineStateMachineBoxResult(currentState.ForkCount, error);
            } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

            if (newState is null) {
                SetExceptionCore(error);
            }
        }

        /// <summary>Gets the status of the box.</summary>
        public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

        ///// <summary>Gets the status of the box.</summary>
        //public ValueTaskSourceStatus GetStatus(short token)
        //{
        //    //return _valueTaskSource.GetStatus(token);
        //    return ValueTaskSourceStatus.Pending;
        //}

        /// <summary>Schedules the continuation action for this box.</summary>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
            _valueTaskSource.OnCompleted(continuation, state, token, flags);

        /// <summary>Gets the current version number of the box.</summary>
        public short Version => _valueTaskSource.Version;

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
    }

    /// <summary>Type used as a singleton to indicate synchronous success for an async method.</summary>
    private sealed class SynchronousSuccessSentinelCoroutineStateMachineBox : CoroutineStateMachineBox, ICoroutineResultStateMachineBox
    {
        public SynchronousSuccessSentinelCoroutineStateMachineBox() => SetResultCore(default!);

        void ICoroutineResultStateMachineBox.CallbackWhenForkCompletedUnsafely<TAwaiter>(ref TAwaiter awaiter, Action continuation) =>
            awaiter.UnsafeOnCompleted(continuation);
    }

    /// <summary>Provides a strongly-typed box object based on the specific state machine type in use.</summary>
    internal sealed class CoroutineStateMachineBox<TStateMachine> : CoroutineStateMachineBox, IValueTaskSource<TResult>, IValueTaskSource, 
        ICoroutineStateMachineBox, IThreadPoolWorkItem, ICoroutineResultStateMachineBox, IAsyncIteratorStateMachineBox<TResult>
        where TStateMachine : IAsyncStateMachine
    {
        /// <summary>Delegate used to invoke on an ExecutionContext when passed an instance of this box type.</summary>
        private static readonly ContextCallback s_callback = ExecutionContextCallback;

        /// <summary>Per-core cache of boxes, with one box per core.</summary>
        /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
        private static readonly PaddedReference[] s_perCoreCache = new PaddedReference[Environment.ProcessorCount];

        /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
        [ThreadStatic]
        private static CoroutineStateMachineBox<TStateMachine>? t_tlsCache;

        /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // only one caller
        internal static CoroutineStateMachineBox<TStateMachine> RentFromCache()
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
                    box = new CoroutineStateMachineBox<TStateMachine>();
                }
            }

            box.Initialize();
            return box;
        }

        /// <summary>The state machine itself.</summary>
        public TStateMachine? StateMachine;

        /// <summary>A delegate to the <see cref="MoveNext()"/> method.</summary>
        public Action MoveNextAction => _moveNextAction ??= new Action(MoveNext);

        ref CoroutineContext ICoroutineStateMachineBox.CoroutineContext => ref _coroutineContext;

        private void Initialize()
        {
            _coroutineContext.SetResultStateMachine(this);
            _result = CoroutineStateMachineBoxResult.Default;
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

        void ICoroutineResultStateMachineBox.CallbackWhenForkCompletedUnsafely<TAwaiter>(ref TAwaiter forkAwaiter, Action forkCompleted)
        {
            CoroutineStateMachineBoxResult? currentState;
            CoroutineStateMachineBoxResult newState;

            do {
                currentState = _result;

                if (currentState is null || currentState.State != CoroutineStateMachineBoxResult.ResultState.NotYetComputed) {
                    throw new InvalidOperationException("Result state machine has already finished");
                }

                newState = new CoroutineStateMachineBoxResult(currentState.ForkCount + 1);
            } while (!ReferenceEquals(Interlocked.CompareExchange(ref _result, newState, currentState), currentState));

            forkAwaiter.UnsafeOnCompleted(() => {
                Exception? childError = null;

                try {
                    forkCompleted();
                } catch (Exception error) {
                    childError = error;
                }

                CoroutineStateMachineBoxResult? currentState;
                CoroutineStateMachineBoxResult? newState;

                do {
                    currentState = _result;

                    if (currentState is null) {
                        return;
                    }

                    if (currentState.ForkCount == 1 && currentState.State != CoroutineStateMachineBoxResult.ResultState.NotYetComputed) {
                        newState = null;
                    } else {
                        newState = new CoroutineStateMachineBoxResult(currentState, currentState.ForkCount - 1);
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

        /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
        private static ref CoroutineStateMachineBox<TStateMachine>? PerCoreCacheSlot {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers are RentFrom/ReturnToCache
            get {
                // Get the current processor ID.  We need to ensure it fits within s_perCoreCache, so we
                // could % by its length, but we can do so instead by Environment.ProcessorCount, which will be a const
                // in tier 1, allowing better code gen, and then further use uints for even better code gen.
                Debug.Assert(s_perCoreCache.Length == Environment.ProcessorCount, $"{s_perCoreCache.Length} != {Environment.ProcessorCount}");
                int i = (int)((uint)Thread.GetCurrentProcessorId() % (uint)Environment.ProcessorCount);

                // We want an array of StateMachineBox<> objects, each consuming its own cache line so that
                // elements don't cause false sharing with each other.  But we can't use StructLayout.Explicit
                // with generics.  So we use object fields, but always reinterpret them (for all reads and writes
                // to avoid any safety issues) as StateMachineBox<> instances.
#if DEBUG
                object? transientValue = s_perCoreCache[i].Object;
                Debug.Assert(transientValue is null || transientValue is CoroutineStateMachineBox<TStateMachine>,
                    $"Expected null or {nameof(CoroutineStateMachineBox<TStateMachine>)}, got '{transientValue}'");
#endif
                return ref Unsafe.As<object?, CoroutineStateMachineBox<TStateMachine>?>(ref s_perCoreCache[i].Object);
            }
        }

        /// <summary>
        /// Used to initialize s_callback above. We don't use a lambda for this on purpose: a lambda would
        /// introduce a new generic type behind the scenes that comes with a hefty size penalty in AOT builds.
        /// </summary>
        private static void ExecutionContextCallback(object? s)
        {
            // Only used privately to pass directly to EC.Run
            Debug.Assert(s is CoroutineStateMachineBox<TStateMachine>, $"Expected {nameof(CoroutineStateMachineBox<TStateMachine>)}, got '{s}'");
            Unsafe.As<CoroutineStateMachineBox<TStateMachine>>(s).StateMachine!.MoveNext();
        }

        /// <summary>Calls MoveNext on <see cref="StateMachine"/></summary>
        public void MoveNext()
        {
            ExecutionContext? context = _executionContext;

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

        void IAsyncIteratorStateMachineBox<TResult>.SetAsyncIteratorCompletionSource(IValueTaskCompletionSource<TResult>? completionSource) =>
            _valueTaskSource._asyncIteratorCompletionSource = completionSource;

        void IAsyncIteratorStateMachineBox<TResult>.SetResult(TResult result) => _valueTaskSource._valueTaskSource.SetResult(result);

        void IAsyncIteratorStateMachineBox<TResult>.SetException(Exception e) => _valueTaskSource._valueTaskSource.SetException(e);
    }

    internal class CoroutineStateMachineBoxResult : IEquatable<CoroutineStateMachineBoxResult>
    {
        public static readonly CoroutineStateMachineBoxResult Default = new();

        private CoroutineStateMachineBoxResult()
        {
            Result = default!;
        }

        public CoroutineStateMachineBoxResult(int forkCount)
        {
            Result = default!;
            ForkCount = forkCount;
        }

        public CoroutineStateMachineBoxResult(int forkCount, TResult result)
        {
            ForkCount = forkCount;
            State = ResultState.HasResult;
            Result = result;
        }

        public CoroutineStateMachineBoxResult(int forkCount, Exception error)
        {
            ForkCount = forkCount;
            State = ResultState.HasError;
            Result = default!;
            Error = error;
        }

        public CoroutineStateMachineBoxResult(CoroutineStateMachineBoxResult original, int forkCount)
        {
            ForkCount = forkCount;
            State = original.State;
            Result = original.Result;
            Error = original.Error;
        }

        public int ForkCount { get; init; }

        [MemberNotNullWhen(true, nameof(Result))]
        public bool HasResult {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return State.HasFlag(ResultState.HasResult);
            }
        }

        [MemberNotNullWhen(true, nameof(Error))]
        public bool HasError {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return State.HasFlag(ResultState.HasError);
            }
        }

        public ResultState State { get; init; }
        public TResult Result { get; init; }
        public Exception? Error { get; init; }

        public bool Equals(CoroutineStateMachineBoxResult? other)
        {
            if (other is null) {
                return false;
            }

            return ForkCount == other.ForkCount &&
                State == other.State;
        }

        internal enum ResultState : byte
        {
            NotYetComputed = 0,
            HasResult = 1,
            HasError = 2
        }
    }
}
