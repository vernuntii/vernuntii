using System.Collections.Immutable;
using Vernuntii.Reactive.Coroutines.AsyncEffects;

namespace Vernuntii.Reactive.Coroutines;

public class CoroutineExecutorBuilder
{
    private List<IEffectStore> _stepStores = new List<IEffectStore>();
    private ImmutableHashSet<EffectHandlerId> _storedSteps = ImmutableHashSet<EffectHandlerId>.Empty;

    public CoroutineExecutorBuilder AddStepStore(IEffectStore store)
    {
        var expectedNewStoredStepsCount = _storedSteps.Count + store.CompiledEffects.Count;
        var actualNewStoredSteps = _storedSteps.Union(store.CompiledEffects);

        if (expectedNewStoredStepsCount != actualNewStoredSteps.Count) {
            throw new InvalidOperationException();
        }

        _storedSteps = actualNewStoredSteps;
        _stepStores.Add(store);
        return this;
    }

    public ICoroutineExecutor Build()
    {
        var steps = _stepStores
            .SelectMany(x => x.CompiledEffects, (x, y) => (StepStore: x, StepId: y))
            .ToDictionary(x => x.StepId, x => x.StepStore);

        return new CoroutineExecutor(steps);
    }
}
