using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

internal static class EffectExtensions
{
    public static Coroutine<EventObserver<T>> Observe<T>(this Effect _, Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        return default;
    }
}
