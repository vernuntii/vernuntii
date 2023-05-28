namespace Vernuntii.Reactive.Coroutines.Steps;

interface ICatchStep : IStep
{
    IStep Finally();
}
