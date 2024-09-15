namespace Vernuntii.Coroutines;
using static Vernuntii.Coroutines.Yielders.Arguments;

file class CoroutineArgumentReceiverAcceptor<T>(T value, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new YieldReturnArgument<T>(value, completionSource);
        argumentReceiver.ReceiveCallableArgument(in YieldReturnKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine YieldReturn<T>(T value)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor<T>(value, completionSource));
    }

    partial class Arguments
    {
        public readonly struct YieldReturnArgument<T> : ICallableArgument
        {
            private readonly T _value;
            private readonly ManualResetValueTaskCompletionSource<T> _completionSource;

            public readonly T Result => _value;

            internal YieldReturnArgument(
                T result,
                ManualResetValueTaskCompletionSource<T> completionSource)
            {
                _value = result;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context) => _completionSource.SetResult(_value);
        }
    }
}
