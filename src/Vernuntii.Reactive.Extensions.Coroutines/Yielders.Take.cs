using Vernuntii.Coroutines;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(EventChannel<T> eventChannel, CancellationToken cancellationToken, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new TakeArgument<T>(eventChannel, cancellationToken);
        argumentReceiver.ReceiveCallableArgument(in EmitKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine<T> Take<T>(EventChannel<T> eventChannel, CancellationToken cancellationToken = default)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(eventChannel, cancellationToken, completionSource));
    }

    partial class Arguments
    {
        public readonly struct TakeArgument<T>(EventChannel<T> eventChannel, CancellationToken cancellationToken) : ICallableArgument
        {
            public EventChannel<T> EventChannel {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eventChannel;
            }

            public CancellationToken CancellationToken {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => cancellationToken;
            }

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource) =>
                EventChannel._channel.Reader.ReadAsync(CancellationToken).DelegateCompletion(Unsafe.As<ManualResetValueTaskCompletionSource<T>>(completionSource));
        }
    }
}
