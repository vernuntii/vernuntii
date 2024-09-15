using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines.CompilerServices;

public readonly struct CoroutineAwaitable
{
    private readonly Coroutine _coroutine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal CoroutineAwaitable(in Coroutine coroutine) => _coroutine = coroutine;

    public CoroutineAwaiter GetAwaiter() => _coroutine.GetAwaiter();

    public ConfiguredCoroutineAwaitable ConfigureAwait(bool continueOnCapturedContext) => _coroutine.ConfigureAwait(continueOnCapturedContext);

    public readonly Task AsTask() => _coroutine._task.AsTask();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Coroutine(CoroutineAwaitable awaitable) => awaitable._coroutine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValueTask(CoroutineAwaitable awaitable) => awaitable._coroutine._task;
}
