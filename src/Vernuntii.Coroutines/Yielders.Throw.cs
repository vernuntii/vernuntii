namespace Vernuntii.Coroutines;

using System.Runtime.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

file class CoroutineArgumentReceiverAcceptor(Exception exception, ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
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
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor(exception, completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly struct ThrowArgument(Exception exception) : ICallableArgument
        {
            public Exception Exception {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => exception;
            }

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource) =>
                Unsafe.As<ManualResetValueTaskCompletionSource<Nothing>>(completionSource).SetException(Exception);
        }
    }
}
