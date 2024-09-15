using System.Reflection;

namespace Vernuntii.Coroutines.Iterators;

public static class CloneableAsyncIterator
{
    public static ICloneableAsyncIterator<TResult> Create<TResult>(Func<Coroutine<TResult>> provider)
    {
        var stateMachineAttribute = provider.Method.GetCustomAttribute<StateMachineAttribute>() ??
            throw new ArgumentException("Coroutine provider must have the async modifier, otherwise the provider won't be linked to a state machine type", nameof(provider));

        return null!;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static ICloneableAsyncIterator Create(Func<Coroutine> provider) => new AsyncIteratorImpl<Nothing>(provider);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static ICloneableAsyncIterator Create(Coroutine coroutine) => new AsyncIteratorImpl<Nothing>(coroutine);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static ICloneableAsyncIterator<TResult> Create<TResult>(Func<Coroutine<TResult>> provider) => new AsyncIteratorImpl<TResult>(provider);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IAsyncIterator<TResult> Create<TResult>(Coroutine<TResult> coroutine) => new AsyncIteratorImpl<TResult>(coroutine);
}
