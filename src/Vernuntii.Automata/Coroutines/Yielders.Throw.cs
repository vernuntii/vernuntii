namespace Vernuntii.Coroutines;

partial class Yielders
{
    public static Coroutine Throw(Exception exception)
    {
        var completionSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.ThrowArgument(exception, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_throwArgumentType, in argument, completionSource);
        }
    }

    partial class Arguments
    {
        internal readonly struct ThrowArgument : ICallableArgument
        {
            private readonly Exception _exception;
            private readonly ValueTaskCompletionSource<Nothing> _completionSource;

            public readonly Exception Exception => _exception;

            internal ThrowArgument(
                Exception exception,
                ValueTaskCompletionSource<Nothing> completionSource)
            {
                _exception = exception;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context) => _completionSource.SetException(_exception);
        }
    }
}
