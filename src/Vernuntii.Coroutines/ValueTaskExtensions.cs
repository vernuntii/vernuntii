namespace Vernuntii.Coroutines;

public static class ValueTaskExtensions
{
    public static Coroutine AsCoroutine(this in ValueTask task) => new(in task);

    public static Coroutine<TResult> AsCoroutine<TResult>(this in ValueTask<TResult> task) => new(in task);
}
