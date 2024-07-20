namespace Vernuntii.Reactive.Coroutines.Stepping;

interface ICatchStep : IStep
{
    IStep Finally();
}
