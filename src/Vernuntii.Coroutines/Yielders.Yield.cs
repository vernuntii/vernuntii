using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor(ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new YieldArgument();
        argumentReceiver.ReceiveCallableArgument(in YieldKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine Yield()
    {
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor(completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly struct YieldArgument() : ICallableArgument
        {
            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource) =>
                new YieldAwaitable.YieldAwaiter().UnsafeOnCompleted(Unsafe.As<ManualResetValueTaskCompletionSource<Nothing>>(completionSource).SetDefaultResult);
        }
    }
}
