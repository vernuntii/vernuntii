using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine Emit<T>(this Effect _, IEventDiscriminator<T> eventDiscriminator, T eventData)
    {
        return default;
    }

    public static Coroutine Emit(this Effect _, IEventDiscriminator<object?> eventDiscriminator)
    {
        return default;
    }

    partial class Arguments
    {
    }
}
