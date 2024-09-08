using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

delegate void BequestContextDelegate(ref CoroutineContext context, in CoroutineContext contextToBequest);

public struct CoroutineContext : ICoroutinePreprocessor
{
    private static readonly CoroutineContextServiceMap s_emptyKeyedServices = new();

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
        CoroutineContextServiceMap? keyedServices = null,
        CoroutineContextServiceMap? keyedServicesToBequest = null)
    {
        var context = new CoroutineContext();
        context._keyedServices = keyedServices;
        context._keyedServicesToBequest = keyedServicesToBequest;
        return context;
    }

    internal ICoroutineResultStateMachineBox? _resultStateMachine;
    internal CoroutineContextServiceMap? _keyedServices;
    internal CoroutineContextServiceMap? _keyedServicesToBequest;
    internal CoroutineContextBequesterOrigin _bequesterOrigin;
    internal BequestContextDelegate? _bequestContext;
    internal bool _isCoroutineAsyncIteratorSupplier; // To enable fast async iterator check-up
#if DEBUG
    internal int _identifier;
#endif

    internal ICoroutineResultStateMachineBox ResultStateMachine => _resultStateMachine ??= CoroutineMethodBuilder<Nothing>.CoroutineStateMachineBox.s_synchronousSuccessSentinel;

    public CoroutineContextServiceMap KeyedServices => _keyedServices ??= s_emptyKeyedServices;
    public CoroutineContextServiceMap KeyedServicesToBequest => _keyedServicesToBequest ??= s_emptyKeyedServices;
    public readonly CoroutineContextBequesterOrigin BequesterOrigin => _bequesterOrigin;
    public readonly bool IsCoroutineAsyncIteratorSupplier => _isCoroutineAsyncIteratorSupplier;

    internal CoroutineScope Scope {
        get {
            Debug.Assert(KeyedServicesToBequest.Contains(CoroutineScope.s_coroutineScopeKey));
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

    readonly void ICoroutinePreprocessor.PreprocessChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
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
    }
}
