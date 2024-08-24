using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines;

public struct CoroutineContext : ICoroutinePreprocessor
{
    private static readonly IReadOnlyDictionary<Key, object> s_emptyKeyedServices = new Dictionary<Key, object>();

    internal static readonly Key s_coroutineScopeKey = new Key(Encoding.ASCII.GetBytes(nameof(CoroutineScope)), isContextService: true, inheritable: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CoroutineContext CreateInternal(
        IReadOnlyDictionary<Key, object>? keyedServices = null,
        IReadOnlyDictionary<Key, object>? keyedServicesToBequest = null) =>
        new CoroutineContext(keyedServices, keyedServicesToBequest);

    private ICoroutineResultStateMachine? _resultStateMachine;
    private IReadOnlyDictionary<Key, object>? _keyedServices;
    private IReadOnlyDictionary<Key, object>? _keyedServicesToBequest;
    internal CoroutineContextBequeathBehaviour _bequeathBehaviour;
#if DEBUG
    internal int _identifier;
#endif

    internal ICoroutineResultStateMachine ResultStateMachine => _resultStateMachine ??= CoroutineResultStateMachine.s_immediateContinuingResultStateMachine;
    internal IReadOnlyDictionary<Key, object> KeyedServices => _keyedServices ??= s_emptyKeyedServices;
    internal IReadOnlyDictionary<Key, object> KeyedServicesToBequest => _keyedServicesToBequest ??= s_emptyKeyedServices;

    internal CoroutineScope Scope {
        get {
            Debug.Assert(KeyedServicesToBequest.ContainsKey(s_coroutineScopeKey));
            return (CoroutineScope)KeyedServicesToBequest[s_coroutineScopeKey];
        }
    }

    internal CoroutineContext(IReadOnlyDictionary<Key, object>? keyedServices, IReadOnlyDictionary<Key, object>? keyedServicesToBequest)
    {
        if (keyedServices is not null) {
            _keyedServices = keyedServices;
        }
        if (keyedServicesToBequest is not null) {
            _keyedServicesToBequest = keyedServicesToBequest;
        }
    }

    internal readonly void BequestContext(ref CoroutineContext childContext)
    {
        if ((_bequeathBehaviour & CoroutineContextBequeathBehaviour.PrivateBequestingUntilChild) != 0) {
            childContext._keyedServices = _keyedServices;
        }

        childContext._keyedServicesToBequest = _keyedServicesToBequest;

        if ((childContext._bequeathBehaviour & CoroutineContextBequeathBehaviour.NoPrivateBequesting) != 0) {
            childContext._bequeathBehaviour = _bequeathBehaviour;
        }
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
        _bequeathBehaviour = CoroutineContextBequeathBehaviour.Undefined;
        _keyedServicesToBequest = null!;
        _keyedServices = null!;
        _resultStateMachine = null!;
    }
}
