using Vernuntii.Reactive.Coroutines.AsyncEffects;

namespace Vernuntii.Reactive.Coroutines;

internal interface IEventConnection : IDisposable
{
    IEventTrace Trace { get; }
}
