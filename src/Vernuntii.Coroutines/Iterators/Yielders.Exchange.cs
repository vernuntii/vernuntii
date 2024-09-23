using static Vernuntii.Coroutines.Iterators.Yielders.Arguments;

namespace Vernuntii.Coroutines.Iterators;

file class CoroutineArgumentReceiverAcceptor<T>(T result, ManualResetCoroutineCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ExchangeArgument<T>(result);
        argumentReceiver.ReceiveCallableArgument(in ExchangeKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine<T> Exchange<T>(T value)
    {
        var completionSource = ManualResetCoroutineCompletionSource<T>.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(value, completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly struct ExchangeArgument<T>(T result) : ICallableArgument<ManualResetCoroutineCompletionSource<T>>
        {
            public T Result {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => result;
            }

            void ICallableArgument<ManualResetCoroutineCompletionSource<T>>.Callback(in CoroutineContext context, ManualResetCoroutineCompletionSource<T> completionSource) =>
                completionSource.SetResult(Result);
        }
    }
}
