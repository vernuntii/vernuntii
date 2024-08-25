namespace Vernuntii.Coroutines;

internal interface ICoroutineContext
{
    bool WantsToBequest { get; }

    ICoroutineResultStateMachine ResultStateMachine { get; }
    IReadOnlyDictionary<Key, object> KeyedServices { get; }
    IReadOnlyDictionary<Key, object> KeyedServicesToBequest { get; }

    void InheritContext<TContext>(ref TContext context) where TContext : ICoroutineContext;
    void BequestContext<TContext>(ref TContext context) where TContext : ICoroutineContext;
}
