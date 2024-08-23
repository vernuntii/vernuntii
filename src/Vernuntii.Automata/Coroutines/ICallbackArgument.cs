namespace Vernuntii.Coroutines;

internal interface ICallbackArgument
{
    void Callback(ref CoroutineContext coroutineContext);
}
