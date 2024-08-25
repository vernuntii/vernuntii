using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

delegate void BequestContextDelegate(ref CoroutineContext context, ref CoroutineContext contextToBequest);

public struct CoroutineContext : ICoroutinePreprocessor
{
    private static readonly IReadOnlyDictionary<Key, object> s_emptyKeyedServices = new Dictionary<Key, object>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineContext CreateInternal(
        IReadOnlyDictionary<Key, object>? keyedServices = null,
        IReadOnlyDictionary<Key, object>? keyedServicesToBequest = null)
    {
        var context = new CoroutineContext();
        context._keyedServices = keyedServices;
        context._keyedServicesToBequest = keyedServicesToBequest;
        return context;
    }

    internal ICoroutineResultStateMachine? _resultStateMachine;
    internal IReadOnlyDictionary<Key, object>? _keyedServices;
    internal IReadOnlyDictionary<Key, object>? _keyedServicesToBequest;
    internal CoroutineContextBequesterOrigin _bequesterOrigin;
    internal BequestContextDelegate? _bequestContext;
    internal bool? _isAsyncIteratorSupplier;
#if DEBUG
    internal int _identifier;
#endif

    internal ICoroutineResultStateMachine ResultStateMachine => _resultStateMachine ??= CoroutineResultStateMachine.s_immediateContinuingResultStateMachine;

    public IReadOnlyDictionary<Key, object> KeyedServices => _keyedServices ??= s_emptyKeyedServices;
    public IReadOnlyDictionary<Key, object> KeyedServicesToBequest => _keyedServicesToBequest ??= s_emptyKeyedServices;
    readonly public CoroutineContextBequesterOrigin BequesterOrigin => _bequesterOrigin;
    public bool IsAsyncIteratorAware => _isAsyncIteratorSupplier ??= KeyedServices.ContainsKey(AsyncIterator.s_asyncIteratorKey);

    internal CoroutineScope Scope {
        get {
            Debug.Assert(KeyedServicesToBequest.ContainsKey(CoroutineScope.s_coroutineScopeKey));
            return (CoroutineScope)KeyedServicesToBequest[CoroutineScope.s_coroutineScopeKey];
        }
    }

    internal void InheritContext(ref CoroutineContext contextToBequest)
    {
        if ((contextToBequest.BequesterOrigin & CoroutineContextBequesterOrigin.ContextBequester) != 0) {
            _keyedServices = contextToBequest.KeyedServices;
        }

        _keyedServicesToBequest = contextToBequest.KeyedServicesToBequest;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void OnCoroutineStarted()
    {
#if DEBUG
        _identifier = Scope.OnCoroutineStarted();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetResultStateMachine(ICoroutineResultStateMachine resultStateMachine)
    {
        _resultStateMachine = resultStateMachine;
    }

    void ICoroutinePreprocessor.PreprocessChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        coroutineAwaiter.InheritCoroutineContext(ref this);
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
