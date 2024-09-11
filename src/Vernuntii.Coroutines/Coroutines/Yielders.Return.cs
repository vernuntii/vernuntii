namespace Vernuntii.Coroutines;
using static Vernuntii.Coroutines.Yielders.Arguments;

file class CoroutineargumentReceiverAcceptor<T>(T value, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ReturnArgument<T>(value, completionSource);
        argumentReceiver.ReceiveCallableArgument(in ReturnKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine Return<T>(T value)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineargumentReceiverAcceptor<T>(value, completionSource));
    }

    partial class Arguments
    {
        internal readonly struct ReturnArgument<T> : ICallableArgument
        {
            private readonly T _value;
            private readonly ManualResetValueTaskCompletionSource<T> _completionSource;

            public readonly T Result => _value;

            internal ReturnArgument(
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
