using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effects
{
    public async static Coroutine<Coroutine> ForkAsync(Func<Coroutine> provider)
    {
        var completionSource = new CoroutineCompletionSource<Coroutine>();
        var forkedCoroutine = await new Coroutine<Coroutine>(completionSource.CreateValueTask(), ArgumentReceiverDelegate);
        return forkedCoroutine;

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new ForkArgument(provider, completionSource);
            argumentReceiver.ReceiveArgument(ref argument, ArgumentType.Default);
        }
    }

    internal ref struct ForkCoroutineAwaiterReceiver(ref CoroutineStackNode coroutineNode)
    {
        private ref CoroutineStackNode _coroutineNode = ref coroutineNode;

        public void ReceiveCoroutineAwaiter<T>(ref T awaiter) where T : ICriticalNotifyCompletion
        {
            ref var coroutineAwaiter = ref Unsafe.As<T, Coroutine.CoroutineAwaiter>(ref awaiter);
            coroutineAwaiter.PropagateCoroutineNode(ref _coroutineNode);
            coroutineAwaiter.StartStateMachine();
        }
    }

    internal struct ForkArgument(Func<Coroutine> provider, CoroutineCompletionSource<Coroutine> completionSource)
    {
        private readonly Func<Coroutine> _provider = provider;

        public void CreateCoroutine(ref ForkCoroutineAwaiterReceiver awaiterReceiver)
        {
            var coroutine = _provider();
            var awaiter = coroutine.GetAwaiter();
            awaiterReceiver.ReceiveCoroutineAwaiter(ref awaiter);
            completionSource.SetResult(coroutine);
        }
    }
}
