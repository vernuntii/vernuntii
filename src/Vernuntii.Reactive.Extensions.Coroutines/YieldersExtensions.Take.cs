using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T> Take<T>(this __co_ext _, EventChannel<T> eventChannel, CancellationToken cancellationToken = default) =>
        Yielders.Take(eventChannel, cancellationToken);
}
