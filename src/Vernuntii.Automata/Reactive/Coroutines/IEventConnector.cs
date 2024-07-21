using Vernuntii.Reactive.Coroutines.AsyncEffects;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnector
{
    public IEventTrace Trace { get; }

    IEventConnection Connect();
}
