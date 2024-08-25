namespace Vernuntii.Coroutines;

internal interface ICallbackArgument
{
    public ICoroutineCompletionSource CompletionSource { get; }

    void Callback(ref CoroutineContext context);
}
