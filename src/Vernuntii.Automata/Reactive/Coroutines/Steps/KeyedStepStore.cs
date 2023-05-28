namespace Vernuntii.Reactive.Coroutines.Steps;

internal abstract class KeyedStepStore : IStepStore
{
    public IReadOnlyCollection<StepHandlerId> CompiledSteps => _compiledSteps ??= StepHandlers.Keys;

    protected abstract Dictionary<StepHandlerId, IStepHandler> StepHandlers { get; }

    private IReadOnlyCollection<StepHandlerId>? _compiledSteps;

    public ValueTask HandleAsync(IStep step)
    {
        if (!StepHandlers.TryGetValue(step.HandlerId, out var stepHandler)) {
            throw new InvalidOperationException($"Step {step.HandlerId} is not stored");
        }

        return stepHandler.HandleAsync(step);
    }
}
