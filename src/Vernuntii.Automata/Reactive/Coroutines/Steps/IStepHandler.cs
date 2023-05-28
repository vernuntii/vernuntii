namespace Vernuntii.Reactive.Coroutines.Steps;

public interface  IStepHandler
{
    ValueTask HandleAsync(IStep step);
}
