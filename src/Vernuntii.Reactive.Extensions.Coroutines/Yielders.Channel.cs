using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector, ManualResetCoroutineCompletionSource<EventChannel<T>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ObserveArgument<T>(eventSelector);
        argumentReceiver.ReceiveCallableArgument(in ChannelKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine<EventChannel<T>> Channel<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = ManualResetCoroutineCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(eventSelector, completionSource));
    }

    partial class Arguments
    {
        public readonly struct ObserveArgument<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector) : ICallableArgument<ManualResetCoroutineCompletionSource<EventChannel<T>>>
        {
            public Func<IReadOnlyEventBroker, IObservableEvent<T>> EventSelector {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eventSelector;
            }

            void ICallableArgument<ManualResetCoroutineCompletionSource<EventChannel<T>>>.Callback(in CoroutineContext context, ManualResetCoroutineCompletionSource<EventChannel<T>> completionSource)
            {
                var eventBroker = context.GetBequestedEventBroker(ServiceKeys.EventBrokerKey);
                var emissions = System.Threading.Channels.Channel.CreateUnbounded<T>();
                EventSelector(eventBroker).Subscribe((emission, writer) => writer.TryWrite(emission), emissions.Writer);
                completionSource.SetResult(new EventChannel<T>(emissions));
            }
        }
    }
}
