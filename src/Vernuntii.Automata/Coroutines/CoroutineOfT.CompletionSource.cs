using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;

namespace Vernuntii.Coroutines;

partial struct Coroutine<T>
{
    /// <summary>The base type for all value task box reusable box objects, regardless of state machine type.</summary>
    internal abstract class AbstractCompletionSource : IValueTaskSource<T>, IValueTaskSource
    {
        /// <summary>A delegate to the MoveNext method.</summary>
        protected Action? _moveNextAction;
        /// <summary>Captured ExecutionContext with which to invoke MoveNext.</summary>
        public ExecutionContext? Context;
        /// <summary>Implementation for IValueTaskSource interfaces.</summary>
        protected ManualResetValueTaskSourceCore<T> _valueTaskSource;

        public ValueTask<T> CreateValueTask() =>
            new ValueTask<T>(this, Version);

        /// <summary>Completes the box with a result.</summary>
        /// <param name="result">The result.</param>
        public void SetResult(T result) =>
            _valueTaskSource.SetResult(result);

        /// <summary>Completes the box with an error.</summary>
        /// <param name="error">The exception.</param>
        public void SetException(Exception error) =>
            _valueTaskSource.SetException(error);

        /// <summary>Gets the status of the box.</summary>
        public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

        /// <summary>Schedules the continuation action for this box.</summary>
        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
            _valueTaskSource.OnCompleted(continuation, state, token, flags);

        /// <summary>Gets the current version number of the box.</summary>
        public short Version => _valueTaskSource.Version;

        /// <summary>Implemented by derived type.</summary>
        T IValueTaskSource<T>.GetResult(short token) => throw new NotImplementedException("");

        /// <summary>Implemented by derived type.</summary>
        void IValueTaskSource.GetResult(short token) => throw new NotImplementedException("");
    }

    /// <summary>Type used as a singleton to indicate synchronous success for an async method.</summary>
    private sealed class SyncSuccessSentinelStateMachineBox : AbstractCompletionSource
    {
        public SyncSuccessSentinelStateMachineBox() => SetResult(default!);
    }

    /// <summary>Provides a strongly-typed box object based on the specific state machine type in use.</summary>
    internal sealed class CompletionSource : AbstractCompletionSource, IValueTaskSource<T>, IValueTaskSource
    {
        /// <summary>Delegate used to invoke on an ExecutionContext when passed an instance of this box type.</summary>
        private static readonly ContextCallback s_callback = ExecutionContextCallback;

        /// <summary>Per-core cache of boxes, with one box per core.</summary>
        /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
        private static readonly PaddedReference[] s_perCoreCache = new PaddedReference[Environment.ProcessorCount];

        /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
        [ThreadStatic]
        private static CompletionSource? t_tlsCache;

        /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // only one caller
        internal static CompletionSource RentFromCache()
        {
            // First try to get a box from the per-thread cache.
            CompletionSource? box = t_tlsCache;
            if (box is not null) {
                t_tlsCache = null;
            } else {
                // If we can't, then try to get a box from the per-core cache.
                ref CompletionSource? slot = ref PerCoreCacheSlot;
                if (slot is null ||
                    (box = Interlocked.Exchange<CompletionSource?>(ref slot, null)) is null) {
                    // If we can't, just create a new one.
                    box = new CompletionSource();
                }
            }

            return box;
        }

        /// <summary>Returns this instance to the cache.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] // only two callers
        private void ReturnToCache()
        {
            // Clear out the state machine and associated context to avoid keeping arbitrary state referenced by
            // lifted locals, and reset the instance for another await.
            ClearStateUponCompletion();
            _valueTaskSource.Reset();

            // If the per-thread cache is empty, store this into it..
            if (t_tlsCache is null) {
                t_tlsCache = this;
            } else {
                // Otherwise, store it into the per-core cache.
                ref CompletionSource? slot = ref PerCoreCacheSlot;
                if (slot is null) {
                    // Try to avoid the write if we know the slot isn't empty (we may still have a benign race condition and
                    // overwrite what's there if something arrived in the interim).
                    Volatile.Write(ref slot, this);
                }
            }
        }

        /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
        private static ref CompletionSource? PerCoreCacheSlot {
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
                Debug.Assert(transientValue is null || transientValue is CompletionSource,
                    $"Expected null or {nameof(CompletionSource)}, got '{transientValue}'");
#endif
                return ref Unsafe.As<object?, CompletionSource?>(ref s_perCoreCache[i].Object);
            }
        }

        /// <summary>
        /// Clear out the state machine and associated context to avoid keeping arbitrary state referenced by lifted locals.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearStateUponCompletion()
        {
            Context = default;
        }

        /// <summary>
        /// Used to initialize s_callback above. We don't use a lambda for this on purpose: a lambda would
        /// introduce a new generic type behind the scenes that comes with a hefty size penalty in AOT builds.
        /// </summary>
        private static void ExecutionContextCallback(object? s)
        {
            // Only used privately to pass directly to EC.Run
            Debug.Assert(s is CompletionSource, $"Expected {nameof(CompletionSource)}, got '{s}'");
            //Unsafe.As<CompletionSource>(s).StateMachine!.MoveNext();
        }

        ///// <summary>A delegate to the <see cref="MoveNext()"/> method.</summary>
        //public Action MoveNextAction => _moveNextAction ??= new Action(MoveNext);

        ///// <summary>Invoked to run MoveNext when this instance is executed from the thread pool.</summary>
        //void IThreadPoolWorkItem.Execute() => MoveNext();

        ///// <summary>Calls MoveNext on <see cref="StateMachine"/></summary>
        //public void MoveNext()
        //{
        //    ExecutionContext? context = Context;

        //    if (context is null) {
        //        Debug.Assert(StateMachine is not null, $"Null {nameof(StateMachine)}");
        //        StateMachine.MoveNext();
        //    } else {

        //        //ExecutionContext.RunInternal(context, s_callback, this);
        //        ExecutionContext.Run(context, s_callback, this);
        //    }
        //}

        /// <summary>Get the result of the operation.</summary>
        T IValueTaskSource<T>.GetResult(short token)
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

        ///// <summary>Gets the state machine as a boxed object.  This should only be used for debugging purposes.</summary>
        //IAsyncStateMachine IAsyncStateMachineBox.GetStateMachineObject() => StateMachine!; // likely boxes, only use for debugging
    }
}

/// <summary>A class for common padding constants and eventually routines.</summary>
internal static class PaddingSizeHolder
{
    /// <summary>A size greater than or equal to the size of the most common CPU cache lines.</summary>
#if TARGET_ARM64 || TARGET_LOONGARCH64
        internal const int CACHE_LINE_SIZE = 128;
#else
    internal const int CACHE_LINE_SIZE = 64;
#endif
}

/// <summary>Padded reference to an object.</summary>
[StructLayout(LayoutKind.Explicit, Size = PaddingSizeHolder.CACHE_LINE_SIZE)]
internal struct PaddedReference
{
    [FieldOffset(0)]
    public object? Object;
}
