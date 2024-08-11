using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial struct Coroutine
{
    /// <summary>Gets a coroutine that has already completed successfully.</summary>
    public static Coroutine CompletedCoroutine => default;

    /// <summary>Creates a <see cref="Coroutine{TResult}"/> that's completed successfully with the specified result.</summary>
    /// <typeparam name="TResult">The type of the result returned by the coroutine.</typeparam>
    /// <param name="result">The result to store into the completed coroutine.</param>
    /// <returns>The successfully completed coroutine.</returns>
    public static Coroutine<TResult> FromResult<TResult>(TResult result) =>
        new Coroutine<TResult>(new ValueTask<TResult>(result));

    /// <summary>Creates a <see cref="Coroutine"/> that has completed due to cancellation with the specified cancellation token.</summary>
    /// <param name="cancellationToken">The cancellation token with which to complete the coroutine.</param>
    /// <returns>The canceled coroutine.</returns>
    public static Coroutine FromCanceled(CancellationToken cancellationToken) =>
        new Coroutine(new ValueTask(Task.FromCanceled(cancellationToken)));

    /// <summary>Creates a <see cref="Coroutine{TResult}"/> that has completed due to cancellation with the specified cancellation token.</summary>
    /// <param name="cancellationToken">The cancellation token with which to complete the coroutine.</param>
    /// <returns>The canceled coroutine.</returns>
    public static Coroutine<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) =>
        new Coroutine<TResult>(new ValueTask<TResult>(Task.FromCanceled<TResult>(cancellationToken)));

    /// <summary>Creates a <see cref="ValueTask"/> that has completed with the specified exception.</summary>
    /// <param name="exception">The exception with which to complete the coroutine.</param>
    /// <returns>The faulted coroutine.</returns>
    public static Coroutine FromException(Exception exception) =>
        new Coroutine(new ValueTask(Task.FromException(exception)));

    /// <summary>Creates a <see cref="Coroutine{TResult}"/> that has completed with the specified exception.</summary>
    /// <param name="exception">The exception with which to complete the coroutine.</param>
    /// <returns>The faulted coroutine.</returns>
    public static Coroutine<TResult> FromException<TResult>(Exception exception) =>
        new Coroutine<TResult>(new ValueTask<TResult>(Task.FromException<TResult>(exception)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StartCoroutine<TCoroutine>(ref TCoroutine coroutine) where TCoroutine : ICoroutine
    {
        var context = new CoroutineContext();
        var node = new CoroutineStackNode(context);
        coroutine.PropagateCoroutineNode(ref node);
        coroutine.StartStateMachine();
    }

    public static async ValueTask<T> RunAsync<T>(Func<Coroutine<T>> provider)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        var coroutine = provider();
        StartCoroutine(ref coroutine);
        return await coroutine;
    }

    public static async ValueTask RunAsync(Func<Coroutine> provider)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        var coroutine = provider();
        StartCoroutine(ref coroutine);
        await coroutine;
    }
}
