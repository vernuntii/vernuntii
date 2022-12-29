using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Reactive;

public static class EventChainExtensions
{
    public static EventChain<(T1, T2)> Zip<T1, T2>(this EventChain<T1> source, EventChain<T2> other) =>
        source.Chain(EventChainFragment.Create(new ZipEvent<T1, T2>(source, other)));

    public static EventChain<(T1, T2)> Zip<T1, T2>(this EventChain<T1> source, IEventDiscriminator<T2> other) =>
        source.Zip(source.Every(other));

    public static EventChain<TResult> Transform<TSource, TResult>(this EventChain<TSource> source, Func<TSource, TResult> transform) =>
        source.Chain(EventChainFragment.Create(new TransformEvent<TSource, TResult>(source, transform)));
}
