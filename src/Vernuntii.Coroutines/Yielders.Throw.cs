namespace Vernuntii.Coroutines;

using static Vernuntii.Coroutines.Yielders.Arguments;

file class CoroutineArgumentReceiverAcceptor(Exception exception, ManualResetCoroutineCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ThrowArgument(exception);
        argumentReceiver.ReceiveCallableArgument(in ThrowKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine Throw(Exception exception)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor(exception, completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly struct ThrowArgument(Exception exception) : ICallableArgument<ManualResetCoroutineCompletionSource<Nothing>>
        {
            public Exception Exception {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => exception;
            }

            void ICallableArgument<ManualResetCoroutineCompletionSource<Nothing>>.Callback(in CoroutineContext context, ManualResetCoroutineCompletionSource<Nothing> completionSource) =>
                completionSource.SetException(Exception);
        }
    }
}
