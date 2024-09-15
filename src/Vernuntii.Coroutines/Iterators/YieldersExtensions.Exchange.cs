namespace Vernuntii.Coroutines.Iterators;

partial class YieldersExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<T> Exchange<T>(this __co_ext _, T value) => Yielders.Exchange(value);
}
