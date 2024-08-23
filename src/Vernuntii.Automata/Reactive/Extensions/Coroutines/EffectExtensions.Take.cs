using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine<T> Take<T>(this Effect _, EventChannel<T> channel)
    {
        return default;
    }

    partial class Arguments
    {
    }
}
