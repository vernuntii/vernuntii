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

    public void ReceiveArgument<TArgument, TArgumentType>(in TArgument argument, in TArgumentType argumentType)
        where TArgumentType : IArgumentType
    {
        ref var forkArgument = ref Unsafe.As<TArgument, Effects.ForkArgument>(ref Unsafe.AsRef(in argument));
        var awaiterReceiver = new Effects.ForkCoroutineAwaiterReceiver(ref _coroutineNode);
        forkArgument.CreateCoroutine(ref awaiterReceiver);
    }
}
