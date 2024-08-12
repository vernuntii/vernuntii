namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public ref struct CoroutineArgumentReceiver
{
    private ref CoroutineStackNode _coroutineNode;

    internal unsafe CoroutineArgumentReceiver(ref CoroutineStackNode coroutineNode)
    {
        _coroutineNode = ref coroutineNode;
    }

    internal void ReceiveCallbackArgument<TArgument, TArgumentType>(in TArgument argument, in TArgumentType argumentType)
        where TArgument : ICallbackArgument
        where TArgumentType : IArgumentType
    {
        if (argumentType.Version == 1) {
            if (default(TArgumentType) != null && argumentType is ArgumentType) {
                argument.Callback(ref _coroutineNode);
            }
        }
    }
}
