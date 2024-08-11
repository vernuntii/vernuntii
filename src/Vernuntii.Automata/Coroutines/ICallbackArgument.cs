namespace Vernuntii.Coroutines;

public interface ICallbackArgument
{
    internal void Callback(ref CoroutineStackNode coroutineNode);
}
