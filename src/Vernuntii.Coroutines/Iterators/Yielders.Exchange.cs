using static Vernuntii.Coroutines.Iterators.Yielders.Arguments;

namespace Vernuntii.Coroutines.Iterators;

file class CoroutineArgumentReceiverAcceptor<T>(T result, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ExchangeArgument<T>(result, completionSource);
        argumentReceiver.ReceiveCallableArgument(in ExchangeKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine<T> Exchange<T>(T value)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(value, completionSource));
    }

    partial class Arguments
    {
        internal readonly struct ExchangeArgument<T> : ICallableArgument
        {
            private readonly T _result;
            private readonly ManualResetValueTaskCompletionSource<T> _completionSource;

            public readonly T Result => _result;

            internal ExchangeArgument(
                T result,
                ManualResetValueTaskCompletionSource<T> completionSource)
            {
                _result = result;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context) => _completionSource.SetResult(_result);
        }
    }
}
