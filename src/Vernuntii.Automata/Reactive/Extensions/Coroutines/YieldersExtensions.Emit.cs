using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    public static Coroutine Emit<T>(this Yielders _, IEventDiscriminator<T> eventDiscriminator, T eventData)
    {
        return default;
    }

    public static Coroutine Emit(this Yielders _, IEventDiscriminator<object?> eventDiscriminator)
    {
        return default;
    }

    partial class Arguments
    {
    }
}
