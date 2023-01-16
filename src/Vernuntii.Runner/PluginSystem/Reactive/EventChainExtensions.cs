namespace Vernuntii.PluginSystem.Reactive;

public static class EventChainExtensions
{
    public static EventChain<(T1, T2)> Zip<T1, T2>(this EventChain<T1> source, EventChain<T2> other) =>
        source.Chain(EventChainFragment.Create(new ZipEvent<T1, T2>(source, other)));

    public static EventChain<(T1, T2)> Zip<T1, T2>(this EventChain<T1> source, IEventDiscriminator<T2> other) =>
        source.Zip(source.Every(other));

    public static EventChain<TResult> Transform<TSource, TResult>(this EventChain<TSource> source, Func<TSource, TResult> transform) =>
        source.Chain(EventChainFragment.Create(new TransformEvent<TSource, TResult>(source, transform)));

    public static EventChain<TSource> Where<TSource, TState>(this EventChain<TSource> source, Func<TSource, TState, bool> eventPredicate, TState state) =>
        source.Chain(EventChainFragment.Create(
            new WhereEvent<TSource, (Func<TSource, TState, bool> Predicate, TState State)>(
                source,
                static (eventData, tuple) => tuple.Predicate(eventData, tuple.State),
                (eventPredicate, state))));

    public static EventChain<T> Where<T>(this EventChain<T> source, Func<T, bool> eventPredicate) =>
        source.Chain(EventChainFragment.Create(
            new WhereEvent<T, Func<T, bool>>(
                source,
                static (eventData, predicate) => predicate(eventData),
                eventPredicate)));

    public static EventChain<T> When<T>(this EventChain<T> source, Func<bool> eventPredicate) =>
        source.Chain(EventChainFragment.Create(
            new WhereEvent<T, Func<bool>>(
                source,
                static (_, predicate) => predicate(),
                eventPredicate)));
}
