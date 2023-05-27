namespace Vernuntii.Reactive.Centralized;

public static class EventChainExtensions
{
    public static EventChain<(T1, T2)> With<T1, T2>(this EventChain<T1> source, EventChain<T2> with) =>
        source.Chain(new WithEvent<T1, T2>(source, with));

    public static EventChain<(T1, T2)> With<T1, T2>(this EventChain<T1> source, EventChainTemplate<T2> with) =>
        source.With(with.GetOrCreateChain(source));

    public static EventChain<TResult> Transform<TSource, TResult>(this EventChain<TSource> source, Func<TSource, TResult> transform) =>
        source.Chain(new TransformEvent<TSource, TResult>(source, transform));

    public static EventChain<TSource> Where<TSource, TState>(this EventChain<TSource> source, Func<TSource, TState, bool> eventPredicate, TState state) =>
        source.Chain(
            new WhereEvent<TSource, (Func<TSource, TState, bool> Predicate, TState State)>(
                source,
                static (eventData, tuple) => tuple.Predicate(eventData, tuple.State),
                (eventPredicate, state)));

    public static EventChain<T> Where<T>(this EventChain<T> source, Func<T, bool> eventPredicate) =>
        source.Chain(
            new WhereEvent<T, Func<T, bool>>(
                source,
                static (eventData, predicate) => predicate(eventData),
                eventPredicate));

    public static EventChain<T> When<T>(this EventChain<T> source, Func<bool> eventPredicate) =>
        source.Chain(
            new WhereEvent<T, Func<bool>>(
                source,
                static (_, predicate) => predicate(),
                eventPredicate));
}
