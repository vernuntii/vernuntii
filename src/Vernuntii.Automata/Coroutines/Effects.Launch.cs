using System.Text;
using Microsoft.VisualBasic;

namespace Vernuntii.Coroutines;

partial class Effects
{
    internal readonly static ArgumentType LaunchArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("launch"));

    public static Coroutine<Coroutine> LaunchAsync(Func<Coroutine> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new LaunchArgument(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in LaunchArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> LaunchAsync<T>(Func<Coroutine<T>> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine<T>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<T>>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new LaunchArgument<T>(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, LaunchArgumentType);
        }
    }

    internal readonly struct LaunchArgument(Func<Coroutine> provider, Coroutine<Coroutine>.CompletionSource immediateCompletionSource) : ICallbackArgument
    {
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
                    throw; // Must bubble up
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
            immediateCompletionSource.SetResult(coroutine);
        }
    }

    internal readonly struct LaunchArgument<T>(Func<Coroutine<T>> provider, Coroutine<Coroutine<T>>.CompletionSource immediateCompletionSource) : ICallbackArgument
    {
        void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
        {
            var coroutine = provider();
            var coroutineAwaiter = coroutine.GetAwaiter();
            var intermediateCompletionSource = Coroutine<T>.CompletionSource.RentFromCache();
            coroutine._task = intermediateCompletionSource.CreateGenericValueTask();
            coroutineNode.ResultStateMachine.AwaitUnsafeOnCompleted(ref coroutineAwaiter, () => {
                try {
                    var result = coroutineAwaiter.GetResult();
                    intermediateCompletionSource.SetResult(result);
                } catch (Exception error) {
                    intermediateCompletionSource.SetException(error);
                    throw; // Must bubble up
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
            immediateCompletionSource.SetResult(coroutine);
        }
    }
}
