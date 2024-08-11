using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines;

partial class Effects
{
    internal readonly static ArgumentType SpawnArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("spawn"));

    public static Coroutine<Coroutine> SpawnAsync(Func<Coroutine> provider)
    {
        var completionSource = new CoroutineCompletionSource<Coroutine>();
        return new Coroutine<Coroutine>(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new SpawnArgument(provider, completionSource);
            argumentReceiver.ReceiveArgument(in argument, in SpawnArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> SpawnAsync<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = new CoroutineCompletionSource<Coroutine<T>>();
        return new Coroutine<Coroutine<T>>(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new SpawnArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveArgument(in argument, in SpawnArgumentType);
        }
    }

    internal ref struct SpawnCoroutineAwaiterReceiver(ref CoroutineStackNode coroutineNode)
    {
        private ref CoroutineStackNode _coroutineNode = ref coroutineNode;

        public void ReceiveCoroutineAwaiter<T>(ref T awaiter) where T : ICriticalNotifyCompletion
        {
            ref var coroutineAwaiter = ref Unsafe.As<T, Coroutine.CoroutineAwaiter>(ref awaiter);
            var context = new CoroutineContext();
            var node = new CoroutineStackNode(context);
            coroutineAwaiter.PropagateCoroutineNode(ref node);
            coroutineAwaiter.StartStateMachine();
        }
    }

    internal struct SpawnArgument(Func<Coroutine> provider, CoroutineCompletionSource<Coroutine> completionSource)
    {
        private readonly Func<Coroutine> _provider = provider;

        public void CreateCoroutine(ref SpawnCoroutineAwaiterReceiver awaiterReceiver)
        {
            var coroutine = _provider();
            var awaiter = coroutine.GetAwaiter();
            awaiterReceiver.ReceiveCoroutineAwaiter(ref awaiter);
            completionSource.SetResult(coroutine);
        }
    }

    internal struct SpawnArgument<T>(Func<Coroutine<T>> provider, CoroutineCompletionSource<Coroutine<T>> completionSource)
    {
        private readonly Func<Coroutine<T>> _provider = provider;

        public void CreateCoroutine(ref SpawnCoroutineAwaiterReceiver awaiterReceiver)
        {
            var coroutine = _provider();
            var awaiter = coroutine.GetAwaiter();
            awaiterReceiver.ReceiveCoroutineAwaiter(ref awaiter);
            completionSource.SetResult(coroutine);
        }
    }
}
