using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal struct CoroutineContextBequester : ICoroutinePreprocessor
{
    private static (Dictionary<Key, object> Larger, IReadOnlyDictionary<Key, object> Smaller) CopyLargestDictionary(
        IReadOnlyDictionary<Key, object> parentContext,
        IReadOnlyDictionary<Key, object> additiveContext)
    {
        if (parentContext.Count >= additiveContext.Count) {
            var larger = new Dictionary<Key, object>(parentContext);
            return (larger, additiveContext);
        } else {
            var larger = new Dictionary<Key, object>(additiveContext);
            return (larger, parentContext);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MergeIntoLeftDictionary(Dictionary<Key, object> left, IReadOnlyDictionary<Key, object> right)
    {
        foreach (var (key, value) in right) {
            left[key] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<Key, object> MergeIntoLeftDictionary((Dictionary<Key, object> left, IReadOnlyDictionary<Key, object> right) dictionaries)
    {
        var left = dictionaries.left;
        MergeIntoLeftDictionary(left, dictionaries.right);
        return left;
    }

    readonly private ICoroutineResultStateMachine _resultStateMachine;
    readonly private IReadOnlyDictionary<Key, object> _keyedServices;
    readonly private IReadOnlyDictionary<Key, object> _keyedServicesToBequest;
    readonly private CoroutineContextBequeathBehaviour _bequeathBehaviour; 
#if DEBUG
    private int _identifier;
#endif

    readonly internal ICoroutineResultStateMachine ResultStateMachine => _resultStateMachine ?? throw new InvalidOperationException();
    readonly internal IReadOnlyDictionary<Key, object> KeyedServices => _keyedServices ?? throw new InvalidOperationException();
    readonly internal IReadOnlyDictionary<Key, object> KeyedServicesToBequest => _keyedServicesToBequest ?? throw new InvalidOperationException();

    public CoroutineContextBequester(CoroutineContext parentContext, CoroutineContext additiveContext, CoroutineContextBequeathBehaviour bequeathBehaviour)
    {
        _resultStateMachine = parentContext.ResultStateMachine;
        _keyedServices = additiveContext.KeyedServices;
        _keyedServicesToBequest = MergeIntoLeftDictionary(CopyLargestDictionary(parentContext.KeyedServicesToBequest, additiveContext.KeyedServicesToBequest));
        _bequeathBehaviour = bequeathBehaviour;
#if DEBUG
        _identifier = parentContext._identifier;
#endif
    }

    void ICoroutinePreprocessor.PreprocessChildCoroutine<TCoroutineAwaiter>(ref TCoroutineAwaiter coroutineAwaiter)
    {
        ref var coroutineContext = ref Unsafe.As<CoroutineContextBequester, CoroutineContext>(ref this);
        coroutineAwaiter.InheritCoroutineContext(ref coroutineContext);
        coroutineAwaiter.StartCoroutine();
    }

    void ICoroutinePreprocessor.PreprocessSiblingCoroutine<TCoroutine>(ref TCoroutine coroutine)
    {
        ref var coroutineContext = ref Unsafe.As<CoroutineContextBequester, CoroutineContext>(ref this);
        var argumentReceiver = new CoroutineArgumentReceiver(ref coroutineContext);
        coroutine.AcceptCoroutineArgumentReceiver(ref argumentReceiver);
    }
}
