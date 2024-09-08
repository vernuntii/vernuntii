using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vernuntii.Coroutines;

namespace Vernuntii.Collections;

internal class FixedSizedArrayPool<TResult>
{
    /// <summary>Per-core cache of boxes, with one box per core.</summary>
    /// <remarks>Each element is padded to expected cache-line size so as to minimize false sharing.</remarks>
    private readonly CacheLineSizePaddedReference[] s_perCoreCache = new CacheLineSizePaddedReference[Environment.ProcessorCount];

    /// <summary>Thread-local cache of boxes. This currently only ever stores one.</summary>
    private ThreadLocal<TResult[]?> t_tlsCache = new();

    public int Size { get; }

    public FixedSizedArrayPool(int size) => Size = size;

    /// <summary>Gets the slot in <see cref="s_perCoreCache"/> for the current core.</summary>
    private ref TResult[]? PerCoreCacheSlot {
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
            var transientValue = s_perCoreCache[i].Object;
            Debug.Assert(transientValue is null || transientValue is TResult[],
                $"Expected null or {typeof(TResult[])}, got '{transientValue}'");
#endif
            return ref Unsafe.As<object?, TResult[]?>(ref s_perCoreCache[i].Object);
        }
    }

    /// <summary>Gets a box object to use for an operation.  This may be a reused, pooled object, or it may be new.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TResult[] Rent()
    {
        // First try to get a box from the per-thread cache.
        var box = t_tlsCache.Value;

        if (box is not null) {
            t_tlsCache.Value = null;
        } else {
            // If we can't, then try to get a box from the per-core cache.
            ref var slot = ref PerCoreCacheSlot;

            if (slot is null || (box = Interlocked.Exchange(ref slot, null)) is null) {
                // If we can't, just create a new one.
                box = new TResult[Size];
            }
        }

        return box;
    }

    /// <summary>Returns this instance to the cache.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(TResult[] array)
    {
        // Clear out the state machine and associated context to avoid keeping arbitrary state referenced by
        // lifted locals, and reset the instance for another await.

        // If the per-thread cache is empty, store this into it..
        if (t_tlsCache.Value is null) {
            t_tlsCache.Value = array;
        } else {
            // Otherwise, store it into the per-core cache.
            ref var slot = ref PerCoreCacheSlot;
            if (slot is null) {
                // Try to avoid the write if we know the slot isn't empty (we may still have a benign race condition and
                // overwrite what's there if something arrived in the interim).
                Volatile.Write(ref slot, array);
            }
        }
    }
}
