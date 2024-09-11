using Vernuntii.Coroutines;
using static Vernuntii.Reactive.Extensions.Coroutines.YieldersExtensions.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineargumentReceiverAcceptor<T>(EventChannel<T> eventChannel, CancellationToken cancellationToken, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new TakeArgument<T>(eventChannel, cancellationToken, completionSource);
        argumentReceiver.ReceiveCallableArgument(in EmitKey, in argument, completionSource);
    }
}

partial class YieldersExtensions
{
    public static Coroutine<T> Take<T>(this Yielders _, EventChannel<T> eventChannel, CancellationToken cancellationToken = default)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), new CoroutineargumentReceiverAcceptor<T>(eventChannel, cancellationToken, completionSource));
    }

    partial class Arguments
    {
        internal readonly struct TakeArgument<T> : ICallableArgument
        {
            private readonly EventChannel<T> _eventChannel;
            private readonly CancellationToken _cancellationToken;
            private readonly ManualResetValueTaskCompletionSource<T> _completionSource;

            public EventChannel<T> EventChannel => _eventChannel;

            internal TakeArgument(
                EventChannel<T> eventChannel,
                CancellationToken cancellationToken,
                ManualResetValueTaskCompletionSource<T> completionSource)
            {
                _eventChannel = eventChannel;
                _cancellationToken = cancellationToken;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext coroutineContext) =>
                _eventChannel._channel.Reader.ReadAsync(_cancellationToken).DelegateCompletion(_completionSource);
        }
    }
}
