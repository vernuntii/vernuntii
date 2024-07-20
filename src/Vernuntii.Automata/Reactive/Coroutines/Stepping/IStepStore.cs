namespace Vernuntii.Reactive.Coroutines.Stepping;

public interface IStepStore
{
    IReadOnlyCollection<StepHandlerId> CompiledSteps { get; }

    ValueTask HandleAsync(IStep step);
}
