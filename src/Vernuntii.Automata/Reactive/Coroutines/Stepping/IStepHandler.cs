namespace Vernuntii.Reactive.Coroutines.Stepping;

public interface  IStepHandler
{
    ValueTask HandleAsync(IStep step);
}
