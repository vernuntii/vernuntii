using Vernuntii.Reactive.Coroutines.Stepping;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnection : IDisposable
{
    IEventTrace Trace { get; }
}
