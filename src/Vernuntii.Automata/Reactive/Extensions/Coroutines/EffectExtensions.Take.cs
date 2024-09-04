using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine<T> Take<T>(this Yielders _, EventChannel<T> channel)
    {
        return default;
    }

    partial class Arguments
    {
    }
}
