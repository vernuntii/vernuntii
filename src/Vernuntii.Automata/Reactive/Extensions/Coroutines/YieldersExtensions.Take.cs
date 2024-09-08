using Vernuntii.Coroutines;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    public static Coroutine<T> Take<T>(this Yielders _, EventChannel<T> eventChannel, CancellationToken cancellationToken = default)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.TakeArgument<T>(eventChannel, cancellationToken, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.EmitKey, in argument, completionSource);
        }
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
