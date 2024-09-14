using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public readonly struct CoroutineAwaitable<TResult>
{
    internal readonly Coroutine<TResult> _coroutine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal CoroutineAwaitable(in Coroutine<TResult> coroutine) => _coroutine = coroutine;

    public CoroutineAwaiter<TResult> GetAwaiter() => _coroutine.GetAwaiter();

    public ConfiguredCoroutineAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) => _coroutine.ConfigureAwait(continueOnCapturedContext);

    public readonly Task<TResult> AsTask() => _coroutine._task.AsTask();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine<TResult>(CoroutineAwaitable<TResult> awaitable) => awaitable._coroutine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValueTask<TResult>(CoroutineAwaitable<TResult> awaitable) => awaitable._coroutine._task;
}
