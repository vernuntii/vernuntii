using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine AsCoroutineInternal(this Task task) =>
        new Coroutine(task);

    public static Coroutine AsCoroutine(this Task task) =>
        task.AsCoroutineInternal();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> AsCoroutineInternal<TResult>(this Task<TResult> task) =>
        new Coroutine<TResult>(task);

    public static Coroutine<TResult> AsCoroutine<TResult>(this Task<TResult> task) =>
        task.AsCoroutineInternal();
}
