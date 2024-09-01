using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>The base type for all value task box reusable box objects, regardless of state machine type.</summary>
internal class ValueTaskCompletionSource<TResult> : IValueTaskSource<TResult>, IValueTaskSource, IAsyncIteratorCompletionSource<TResult>, IAsyncIterationCompletionSource
{
    /// <summary>Per-core cache of boxes, with one box per core.</summary>
    /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
    private static readonly PaddedReference[] s_perCoreCache = new PaddedReference[Environment.ProcessorCount];

    /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
    [ThreadStatic]
    private static ValueTaskCompletionSource<TResult>? t_tlsCache;

    /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // only one caller
    internal static ValueTaskCompletionSource<TResult> RentFromCache()
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
                box = new ValueTaskCompletionSource<TResult>();
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
            ref var slot = ref PerCoreCacheSlot;
            if (slot is null) {
                // Try to avoid the write if we know the slot isn't empty (we may still have a benign race condition and
                // overwrite what's there if something arrived in the interim).
                Volatile.Write(ref slot, this);
            }
        }
    }

    /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
    private static ref ValueTaskCompletionSource<TResult>? PerCoreCacheSlot {
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
            Debug.Assert(transientValue is null || transientValue is ValueTaskCompletionSource<TResult>,
                $"Expected null or {nameof(ValueTaskCompletionSource<TResult>)}, got '{transientValue}'");
#endif
            return ref Unsafe.As<object?, ValueTaskCompletionSource<TResult>?>(ref s_perCoreCache[i].Object);
        }
    }

    /// <summary>A delegate to the MoveNext method.</summary>
    protected Action? _moveNextAction;
    /// <summary>Captured ExecutionContext with which to invoke MoveNext.</summary>
    public ExecutionContext? Context;
    /// <summary>Implementation for IValueTaskSource interfaces.</summary>
    protected ManualResetValueTaskSourceCore<TResult> _valueTaskSource;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask<TResult> CreateGenericValueTask() =>
        new ValueTask<TResult>(this, Version);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ValueTask CreateValueTask() =>
        new ValueTask(this, Version);

    /// <summary>Completes the box with a result.</summary>
    /// <param name="result">The result.</param>
    public void SetResult(TResult result) =>
        _valueTaskSource.SetResult(result);

    void IAsyncIterationCompletionSource.SetResult<TCoroutineResult>(TCoroutineResult result)
    {
        if (result is null) {
            SetResult(default!);
        }

        if (result is not TResult typedResult) {
            throw new ArgumentException($"The result type {typeof(TCoroutineResult)} mismatches the expceted result type {typeof(TResult)}");
        }

        SetResult(typedResult);
    }

    /// <summary>Completes the box with an error.</summary>
    /// <param name="error">The exception.</param>
    public void SetException(Exception error) =>
        _valueTaskSource.SetException(error);

    ///// <summary>Gets the status of the box.</summary>
    //public ValueTaskSourceStatus GetStatus(short token) => _valueTaskSource.GetStatus(token);

    /// <summary>Gets the status of the box.</summary>
    public ValueTaskSourceStatus GetStatus(short token)
    {
        //return _valueTaskSource.GetStatus(token);
        return ValueTaskSourceStatus.Pending;
    }

    /// <summary>Schedules the continuation action for this box.</summary>
    public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) =>
        _valueTaskSource.OnCompleted(continuation, state, token, flags);

    /// <summary>Gets the current version number of the box.</summary>
    public short Version => _valueTaskSource.Version;

    /// <summary>
    /// Clear out the state machine and associated context to avoid keeping arbitrary state referenced by lifted locals.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearStateUponCompletion()
    {
        Context = default;
    }

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
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
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

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
/// <summary>Padded reference to an object.</summary>
[StructLayout(LayoutKind.Explicit, Size = PaddingSizeHolder.CACHE_LINE_SIZE)]
internal struct PaddedReference
{
    [FieldOffset(0)]
    public object? Object;
}
