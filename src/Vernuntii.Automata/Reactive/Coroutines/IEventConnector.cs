using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnector
{
    public IEventTrace Trace { get; }

    IEventConnection Connect();
}
