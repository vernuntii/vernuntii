using System.Text;
using Microsoft.VisualBasic;

namespace Vernuntii.Coroutines;

partial class Effects
{
    internal readonly static ArgumentType ForkArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("fork"));

    public static Coroutine<Coroutine> Fork(Func<Coroutine> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new ForkArgument(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in ForkArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> Fork<T>(Func<Coroutine<T>> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine<T>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<T>>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new ForkArgument<T>(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, ForkArgumentType);
        }
    }

    internal readonly struct ForkArgument(Func<Coroutine> provider, Coroutine<Coroutine>.CompletionSource immediateCompletionSource) : ICallbackArgument
    {
        private readonly Func<Coroutine> _provider = provider;

        void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
        {
            var coroutine = provider();
            var coroutineAwaiter = coroutine.GetAwaiter();
            var intermediateCompletionSource = Coroutine<object?>.CompletionSource.RentFromCache();
            coroutine._task = intermediateCompletionSource.CreateValueTask();
            coroutineNode.ResultStateMachine.AwaitUnsafeOnCompleted(ref coroutineAwaiter, () => {
                try {
                    coroutineAwaiter.GetResult();
                    intermediateCompletionSource.SetResult(default);
                } catch (Exception error) {
                    intermediateCompletionSource.SetException(error);
                    throw; // Fork must bubble up its error
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
            immediateCompletionSource.SetResult(coroutine);
        }
    }

    internal readonly struct ForkArgument<T>(Func<Coroutine<T>> provider, Coroutine<Coroutine<T>>.CompletionSource immediateCompletionSource) : ICallbackArgument
    {
        private readonly Func<Coroutine<T>> _provider = provider;

        void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
        {
            var coroutine = _provider();
            var coroutineAwaiter = coroutine.GetAwaiter();
            var intermediateCompletionSource = Coroutine<T>.CompletionSource.RentFromCache();
            coroutine._task = intermediateCompletionSource.CreateGenericValueTask();
            coroutineNode.ResultStateMachine.AwaitUnsafeOnCompleted(ref coroutineAwaiter, () => {
                try {
                    var result = coroutineAwaiter.GetResult();
                    intermediateCompletionSource.SetResult(result);
                } catch (Exception error) {
                    intermediateCompletionSource.SetException(error);
                    throw; // Fork must bubble up its error
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
            immediateCompletionSource.SetResult(coroutine);
        }
    }
}
