using System.Text;

namespace Vernuntii.Coroutines;

partial class Effects
{
    public static Coroutine<Coroutine> Spawn(Func<Coroutine> provider)
    {
        var completionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.SpawnArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> Spawn<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = Coroutine<Coroutine<T>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<T>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.SpawnArgumentType);
        }
    }

    partial class Arguments
    {
        internal readonly static ArgumentType SpawnArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("spawn"));

        internal readonly struct SpawnArgument(Func<Coroutine> provider, Coroutine<Coroutine>.CompletionSource completionSource) : ICallbackArgument
        {
            unsafe void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
            {
                var coroutine = provider();
                Coroutine coroutineAsComplementary;

                var coroutineNodeAsCompletary = new CoroutineStackNode(coroutineNode.Context);
                if (!coroutine.IsChildCoroutine) {
                    coroutineAsComplementary = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                } else {
                    coroutineAsComplementary = coroutine;
                }
                var coroutineAsComplementaryAwaiter = coroutineAsComplementary.GetAwaiter();

                var intermediateCompletionSource = Coroutine<object?>.CompletionSource.RentFromCache();
                coroutineAsComplementary._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                coroutineAsComplementaryAwaiter.UnsafeOnCompleted(() => {
                    try {
                        coroutineAsComplementaryAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                completionSource.SetResult(coroutineAsComplementary);
            }
        }

        internal readonly struct SpawnArgument<T>(Func<Coroutine<T>> provider, Coroutine<Coroutine<T>>.CompletionSource completionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
            {
                var coroutine = provider();
                Coroutine<T> coroutineAsComplementary;

                var coroutineNodeAsCompletary = new CoroutineStackNode(coroutineNode.Context);
                if (!coroutine.IsChildCoroutine) {
                    coroutineAsComplementary = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                } else {
                    coroutineAsComplementary = coroutine;
                }
                var coroutineAsComplementaryAwaiter = coroutineAsComplementary.GetAwaiter();

                var intermediateCompletionSource = Coroutine<T>.CompletionSource.RentFromCache();
                coroutineAsComplementary._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                coroutineAsComplementaryAwaiter.UnsafeOnCompleted(() => {
                    try {
                        var result = coroutineAsComplementaryAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                completionSource.SetResult(coroutineAsComplementary);
            }
        }
    }
}
