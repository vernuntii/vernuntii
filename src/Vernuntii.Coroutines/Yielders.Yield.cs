using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor(ManualResetCoroutineCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
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
        var completionSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor(completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly struct YieldArgument() : ICallableArgument<ManualResetCoroutineCompletionSource<Nothing>>
        {
            void ICallableArgument<ManualResetCoroutineCompletionSource<Nothing>>.Callback(in CoroutineContext context, ManualResetCoroutineCompletionSource<Nothing> completionSource) =>
                new YieldAwaitable.YieldAwaiter().UnsafeOnCompleted(completionSource.SetDefaultResult);
        }
    }
}
