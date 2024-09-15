namespace Vernuntii.Coroutines;

public static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine AsCoroutine(this Task task) => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine<TResult> AsCoroutine<TResult>(this Task<TResult> task) => new(task);
}
