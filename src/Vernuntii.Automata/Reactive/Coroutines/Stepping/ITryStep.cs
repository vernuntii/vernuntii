namespace Vernuntii.Reactive.Coroutines.Stepping;

interface ITryStep
{
    ICatchStep Catch();
    IStep Finally();
}
