using System.Text;

namespace Vernuntii.Coroutines;

partial class Effect
{
    public static Coroutine<Coroutine> Launch(Func<Coroutine> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.LaunchArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> Launch<T>(Func<Coroutine<T>> provider)
    {
        var immediateCompletionSource = Coroutine<Coroutine<T>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<T>>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<T>(provider, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, Arguments.LaunchArgumentType);
        }
    }

    partial class Arguments
    {
        internal readonly static ArgumentType LaunchArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("launch"));

        internal readonly struct LaunchArgument(Func<Coroutine> provider, Coroutine<Coroutine>.CompletionSource immediateCompletionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
            {
                var coroutine = provider();
                var coroutineAwaiter = coroutine.GetAwaiter();
                var intermediateCompletionSource = Coroutine<object?>.CompletionSource.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineNode);
                coroutineNode.ResultStateMachine.AwaitUnsafeOnCompletedThenContinueWith(ref coroutineAwaiter, () => {
                    try {
                        coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsHandled();
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
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineNode);
                coroutineNode.ResultStateMachine.AwaitUnsafeOnCompletedThenContinueWith(ref coroutineAwaiter, () => {
                    try {
                        var result = coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsHandled();
                immediateCompletionSource.SetResult(coroutine);
            }
        }
    }
}
