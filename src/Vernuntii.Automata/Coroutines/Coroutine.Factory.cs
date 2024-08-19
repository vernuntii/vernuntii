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

    public static Coroutine Start(Func<Coroutine> provider)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        var coroutine = provider();
        var coroutineContext = new CoroutineContext();
        var coroutineNode = new CoroutineStackNode(coroutineContext);
        CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNode);
        return coroutine;
    }

    public static Coroutine<T> Start<T>(Func<Coroutine<T>> provider)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        var coroutine = provider();
        var coroutineContext = new CoroutineContext();
        var coroutineNode = new CoroutineStackNode(coroutineContext);
        CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNode);
        return coroutine;
    }
}
