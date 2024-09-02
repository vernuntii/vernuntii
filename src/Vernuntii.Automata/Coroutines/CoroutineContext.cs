using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

delegate void BequestContextDelegate(ref CoroutineContext context, in CoroutineContext contextToBequest);

public struct CoroutineContext : ICoroutinePreprocessor
{
    private static readonly ImmutableDictionary<Key, object> s_emptyKeyedServices = ImmutableDictionary<Key, object>.Empty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void InheritOrBequestCoroutineContext(ref CoroutineContext context, in CoroutineContext contextToBequest)
    {
        if (contextToBequest._bequestContext is not null) {
            contextToBequest._bequestContext(ref context, in contextToBequest);
        } else {
            context.InheritContext(in contextToBequest);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineContext CreateInternal(
        ImmutableDictionary<Key, object>? keyedServices = null,
        ImmutableDictionary<Key, object>? keyedServicesToBequest = null)
    {
        var context = new CoroutineContext();
        context._keyedServices = keyedServices;
        context._keyedServicesToBequest = keyedServicesToBequest;
        return context;
    }

    internal ICoroutineResultStateMachineBox? _resultStateMachine;
    internal ImmutableDictionary<Key, object>? _keyedServices;
    internal ImmutableDictionary<Key, object>? _keyedServicesToBequest;
    internal CoroutineContextBequesterOrigin _bequesterOrigin;
    internal BequestContextDelegate? _bequestContext;
    internal bool _isAsyncIteratorSupplier; // Enables fast async iterator check-ups
#if DEBUG
    internal int _identifier;
#endif

    internal ICoroutineResultStateMachineBox ResultStateMachine => _resultStateMachine ??= CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox.s_synchronousSuccessSentinel;

    public ImmutableDictionary<Key, object> KeyedServices => _keyedServices ??= s_emptyKeyedServices;
    public ImmutableDictionary<Key, object> KeyedServicesToBequest => _keyedServicesToBequest ??= s_emptyKeyedServices;
    public readonly CoroutineContextBequesterOrigin BequesterOrigin => _bequesterOrigin;
    public readonly bool IsAsyncIteratorSupplier => _isAsyncIteratorSupplier;

    internal CoroutineScope Scope {
        get {
            Debug.Assert(KeyedServicesToBequest.ContainsKey(CoroutineScope.s_coroutineScopeKey));
            return (CoroutineScope)KeyedServicesToBequest[CoroutineScope.s_coroutineScopeKey];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal void InheritContext(in CoroutineContext contextToBequest)
    {
        if ((contextToBequest.BequesterOrigin & CoroutineContextBequesterOrigin.ContextBequester) != 0) {
            if (_keyedServices is null) {
                _keyedServices = contextToBequest.KeyedServices;
            } else if (contextToBequest._keyedServices is not null) {
                _keyedServices = _keyedServices.Merge(contextToBequest._keyedServices);
            }
        }

        if (_keyedServicesToBequest is null) {
            _keyedServicesToBequest = contextToBequest.KeyedServicesToBequest;
        } else if (contextToBequest._keyedServicesToBequest is not null) {
            _keyedServicesToBequest = _keyedServicesToBequest.Merge(contextToBequest._keyedServicesToBequest);
        }

        _bequestContext = contextToBequest._bequestContext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OnCoroutineStarted()
    {
#if DEBUG
        _identifier = Scope.OnCoroutineStarted();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachineBox resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    void ICoroutinePreprocessor.PreprocessChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.InheritCoroutineContext(in this);
        coroutineAwaiter.StartCoroutine();
    }

    void ICoroutinePreprocessor.PreprocessSiblingCoroutine<TCoroutine>(ref TCoroutine coroutine)
    {
        var argumentReceiver = new CoroutineArgumentReceiver(ref this);
        coroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }

    public void OnCoroutineCompleted()
    {
#if DEBUG
        Scope.OnCoroutineCompleted();
#endif
        _keyedServicesToBequest = null!;
        _keyedServices = null!;
        _resultStateMachine = null!;
    }
}
