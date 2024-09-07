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
    public static Coroutine<TResult> FromResult<TResult>(TResult result) => new(new ValueTask<TResult>(result));

    /// <summary>Creates a <see cref="Coroutine"/> that has completed due to cancellation with the specified cancellation token.</summary>
    /// <param name="cancellationToken">The cancellation token with which to complete the coroutine.</param>
    /// <returns>The canceled coroutine.</returns>
    public static Coroutine FromCanceled(CancellationToken cancellationToken) => new(new ValueTask(Task.FromCanceled(cancellationToken)));

    /// <summary>Creates a <see cref="Coroutine{TResult}"/> that has completed due to cancellation with the specified cancellation token.</summary>
    /// <param name="cancellationToken">The cancellation token with which to complete the coroutine.</param>
    /// <returns>The canceled coroutine.</returns>
    public static Coroutine<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) => new(new ValueTask<TResult>(Task.FromCanceled<TResult>(cancellationToken)));

    /// <summary>Creates a <see cref="ValueTask"/> that has completed with the specified exception.</summary>
    /// <param name="exception">The exception with which to complete the coroutine.</param>
    /// <returns>The faulted coroutine.</returns>
    public static Coroutine FromException(Exception exception) => new(new ValueTask(Task.FromException(exception)));

    /// <summary>Creates a <see cref="Coroutine{TResult}"/> that has completed with the specified exception.</summary>
    /// <param name="exception">The exception with which to complete the coroutine.</param>
    /// <returns>The faulted coroutine.</returns>
    public static Coroutine<TResult> FromException<TResult>(Exception exception) => new(new ValueTask<TResult>(Task.FromException<TResult>(exception)));

    private static void StartCore<TCoroutine>(ref TCoroutine coroutine) where TCoroutine : IRelativeCoroutine
    {
        var scope = new CoroutineScope();
        var context = new CoroutineContext {
            _keyedServicesToBequest = CoroutineContextServiceMap.CreateRange(1, scope, static (x, y) => x.OverwriteInternal(new(CoroutineScope.s_coroutineScopeKey, y)))
        };
        CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutine, ref context);
    }

    private static Coroutine StartInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        Coroutine coroutine;
        if (isProviderWithClosure) {
            coroutine = Unsafe.As<Delegate, Func<TClosure, Coroutine>>(ref provider)(closure);
        } else {
            coroutine = Unsafe.As<Delegate, Func<Coroutine>>(ref provider)();
        }
        StartCore(ref coroutine);
        return coroutine;
    }

    private static Coroutine<TResult> StartInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        ArgumentNullException.ThrowIfNull(nameof(provider));
        Coroutine<TResult> coroutine;
        if (isProviderWithClosure) {
            coroutine = Unsafe.As<Delegate, Func<TClosure, Coroutine<TResult>>>(ref provider)(closure);
        } else {
            coroutine = Unsafe.As<Delegate, Func<Coroutine<TResult>>>(ref provider)();
        }
        StartCore(ref coroutine);
        return coroutine;
    }

    public static Coroutine Start(Func<Coroutine> provider) => StartInternal<object?>(provider, null, isProviderWithClosure: false);

    public static Coroutine Start<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => StartInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> Start<TResult>(Func<Coroutine<TResult>> provider) => StartInternal<object?, TResult>(provider, null, isProviderWithClosure: false);

    public static Coroutine<TResult> Start<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => StartInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);
}
