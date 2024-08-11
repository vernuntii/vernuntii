namespace Vernuntii.Coroutines;

internal interface ICallbackArgument
{
    void Callback(ref CoroutineStackNode coroutineNode);
}
