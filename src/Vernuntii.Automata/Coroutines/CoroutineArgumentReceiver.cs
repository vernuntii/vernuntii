namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public ref struct CoroutineArgumentReceiver
{
    private ref CoroutineContext _coroutineContext;

    internal CoroutineArgumentReceiver(ref CoroutineContext coroutineContext)
    {
        _coroutineContext = ref coroutineContext;
    }

    internal void ReceiveCallbackArgument<TArgument, TArgumentKey>(in TArgument argument, in TArgumentKey argumentKey)
        where TArgument : ICallbackArgument
        where TArgumentKey : IKey
    {
        //if (argumentType.SchemaVersion == 1) {
            //if (default(TArgumentType) != null) {
                argument.Callback(ref _coroutineContext);
            //} else {
            //    throw new NotSupportedException($"The argument type is not of type {typeof(ArgumentType)}");
            //}
        //} else {
        //    throw new NotSupportedException($"The version of the argument type {argumentType.SchemaVersion} is not supported");
        //}
    }
}
