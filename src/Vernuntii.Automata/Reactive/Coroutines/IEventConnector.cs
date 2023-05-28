using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnector
{
    public IEventTrace Trace { get; }

    IEventConnection Connect();
}
