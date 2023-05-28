using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnection : IDisposable
{
    IEventTrace Trace { get; }
}
