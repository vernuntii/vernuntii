namespace Vernuntii.Reactive.Coroutines.Steps;

public interface IStepStore
{
    IReadOnlyCollection<StepHandlerId> CompiledSteps { get; }

    ValueTask HandleAsync(IStep step);
}
