namespace Vernuntii.Coroutines;

internal interface ICallableArgument
{
    public ICoroutineCompletionSource CompletionSource { get; }

    void Callback(in CoroutineContext context);
}
