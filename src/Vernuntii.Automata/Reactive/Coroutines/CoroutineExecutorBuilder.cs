using System.Collections.Immutable;
using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines;

public class CoroutineExecutorBuilder
{
    private List<IStepStore> _stepStores = new List<IStepStore>();
    private ImmutableHashSet<StepHandlerId> _storedSteps = ImmutableHashSet<StepHandlerId>.Empty;

    public CoroutineExecutorBuilder AddSteps(IStepStore store)
    {
        var expectedNewStoredStepsCount = _storedSteps.Count + store.CompiledSteps.Count;
        var actualNewStoredSteps = _storedSteps.Union(store.CompiledSteps);

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
            .SelectMany(x => x.CompiledSteps, (x, y) => (StepStore: x, StepId: y))
            .ToDictionary(x => x.StepId, x => x.StepStore);

        return new CoroutineExecutor(steps);
    }
}
