namespace Vernuntii.Reactive.Coroutines.Steps;

interface ITryStep
{
    ICatchStep Catch();
    IStep Finally();
}
