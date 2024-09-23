using System.ComponentModel;

namespace Vernuntii.Coroutines;

delegate void BequestContextDelegate(ref CoroutineContext context, in CoroutineContext contextToBequest);

file class CoroutineArgumentReceiverAcceptor(ManualResetCoroutineCompletionSource<CoroutineContext> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver) => completionSource.SetResult(argumentReceiver._context);
}

public struct CoroutineContext
{
    internal static CoroutineContext s_statelessCoroutineContext = default;

    private static readonly CoroutineContextServiceMap s_emptyKeyedServices = new();

    public static Coroutine<CoroutineContext> Capture()
    {
        var completionSource = ManualResetCoroutineCompletionSource<CoroutineContext>.RentFromCache();
        return new(completionSource, new CoroutineArgumentReceiverAcceptor(completionSource));
    }

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

    internal ICoroutineResultStateMachineHolder? _resultStateMachine;
    internal CoroutineContextServiceMap? _keyedServices;
    internal CoroutineContextServiceMap? _keyedServicesToBequest;
    internal CoroutineContextBequesterOrigin _bequesterOrigin;
    internal BequestContextDelegate? _bequestContext;
    internal bool _isCoroutineAsyncIteratorSupplier; // To enable fast async iterator check-up
#if DEBUG
    internal int _identifier;
#endif

    internal ICoroutineResultStateMachineHolder ResultStateMachine => _resultStateMachine ??= CoroutineStateMachineHolder<Nothing>.s_synchronousSuccessSentinel;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public CoroutineContextServiceMap KeyedServices => _keyedServices ??= s_emptyKeyedServices;

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public CoroutineContextServiceMap KeyedServicesToBequest => _keyedServicesToBequest ??= s_emptyKeyedServices;

    public readonly CoroutineContextBequesterOrigin BequesterOrigin => _bequesterOrigin;

    internal CoroutineScope? Scope {
        get {
            if (KeyedServicesToBequest.Get(CoroutineScope.s_coroutineScopeKey, out var scope)) {
                return scope as CoroutineScope;
            }

            return null;
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
        if (Scope is { } scope) {
            _identifier = scope.OnCoroutineStarted();
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachineHolder resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    public void OnCoroutineCompleted()
    {
#if DEBUG
        Scope?.OnCoroutineCompleted();
#endif
    }
}
