namespace Vernuntii.Coroutines;

public static class TaskExtensions
{
    public static Coroutine AsCoroutine(this Task task) =>
        new Coroutine(task);

    public static Coroutine<TResult> AsCoroutine<TResult>(this Task<TResult> task) =>
        new Coroutine<TResult>(task);
}
