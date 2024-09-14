namespace Vernuntii.Coroutines;
using static Vernuntii.Coroutines.Yielders.Arguments;

file class CoroutineArgumentReceiverAcceptor(Exception exception, ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ThrowArgument(exception, completionSource);
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
        internal readonly struct ThrowArgument : ICallableArgument
        {
            private readonly Exception _exception;
            private readonly ManualResetValueTaskCompletionSource<Nothing> _completionSource;

            public readonly Exception Exception => _exception;

            internal ThrowArgument(
                Exception exception,
                ManualResetValueTaskCompletionSource<Nothing> completionSource)
            {
                _exception = exception;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context) => _completionSource.SetException(_exception);
        }
    }
}
