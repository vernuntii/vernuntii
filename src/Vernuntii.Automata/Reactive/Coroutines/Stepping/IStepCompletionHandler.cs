namespace Vernuntii.Reactive.Coroutines.Stepping;

internal interface IStepCompletionHandler
{
    IStep Step { get; }

    void CompleteStep();
}
