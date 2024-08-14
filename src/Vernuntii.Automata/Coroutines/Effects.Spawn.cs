using System.Text;

namespace Vernuntii.Coroutines;

partial class Effects
{
    public static Coroutine<Coroutine> Spawn(Func<Coroutine> provider)
    {
        var t = provider();
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
            unsafe void ICallbackArgument.Callback(ref CoroutineStackNode _)
            {
                var coroutine = provider();
                var coroutineContext = new CoroutineContext();
                var coroutineNode = new CoroutineStackNode(coroutineContext);
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNode);
                coroutine.MarkCoroutineAsHandled();
                completionSource.SetResult(coroutine);
            }
        }

        internal readonly struct SpawnArgument<T>(Func<Coroutine<T>> provider, Coroutine<Coroutine<T>>.CompletionSource completionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineStackNode _)
            {
                var coroutine = provider();
                var coroutineContext = new CoroutineContext();
                var coroutineNode = new CoroutineStackNode(coroutineContext);
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNode);
                coroutine.MarkCoroutineAsHandled();
                completionSource.SetResult(coroutine);
            }
        }
    }
}
