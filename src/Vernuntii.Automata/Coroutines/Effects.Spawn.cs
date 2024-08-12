using System.Text;

namespace Vernuntii.Coroutines;

partial class Effects
{
    internal readonly static ArgumentType SpawnArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("spawn"));

    public static Coroutine<Coroutine> SpawnAsync(Func<Coroutine> provider)
    {
        var t =  provider();
        var completionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new SpawnArgument(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in SpawnArgumentType);
        }
    }

    public static Coroutine<Coroutine<T>> SpawnAsync<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = Coroutine<Coroutine<T>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<T>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new SpawnArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in SpawnArgumentType);
        }
    }

    internal readonly struct SpawnArgument(Func<Coroutine> provider, Coroutine<Coroutine>.CompletionSource completionSource) : ICallbackArgument
    {
        unsafe void ICallbackArgument.Callback(ref CoroutineStackNode _)
        {
            var coroutine = provider();
            coroutine.StartOrphanCoroutine();
            coroutine.MarkCoroutineAsHandled();
            completionSource.SetResult(coroutine);
        }
    }

    internal readonly struct SpawnArgument<T>(Func<Coroutine<T>> provider, Coroutine<Coroutine<T>>.CompletionSource completionSource) : ICallbackArgument
    {
        void ICallbackArgument.Callback(ref CoroutineStackNode _)
        {
            var coroutine = provider();
            coroutine.StartOrphanCoroutine();
            coroutine.MarkCoroutineAsHandled();
            completionSource.SetResult(coroutine);
        }
    }
}
