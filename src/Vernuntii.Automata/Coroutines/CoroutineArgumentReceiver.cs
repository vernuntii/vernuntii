using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public delegate void CoroutineArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver);

public ref struct CoroutineArgumentReceiver
{
    private ref CoroutineStackNode _coroutineNode;

    internal unsafe CoroutineArgumentReceiver(ref CoroutineStackNode coroutineNode)
    {
        _coroutineNode = ref coroutineNode;
    }

    public void ReceiveArgument<TArgument, TArgumentType>(in TArgument argument, in TArgumentType argumentType) where TArgumentType : IArgumentType
    {
        if (argumentType.Version == 1) {
            if (default(TArgumentType) != null && argumentType is ArgumentType) {
                var argumentTypeStruct = Unsafe.As<TArgumentType, ArgumentType>(ref Unsafe.AsRef(in argumentType));

                if (argumentTypeStruct.SequenceEqual(in Effects.SpawnArgumentType)) {
                    ref var spawnArgument = ref Unsafe.As<TArgument, Effects.SpawnArgument>(ref Unsafe.AsRef(in argument));
                    var awaiterReceiver = new Effects.SpawnCoroutineAwaiterReceiver(ref _coroutineNode);
                    spawnArgument.CreateCoroutine(ref awaiterReceiver);
                }
            }
        }
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
