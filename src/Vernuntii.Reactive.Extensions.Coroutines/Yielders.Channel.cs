using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector, ManualResetValueTaskCompletionSource<EventChannel<T>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new ObserveArgument<T>(eventSelector, completionSource);
        argumentReceiver.ReceiveCallableArgument(in ChannelKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine<EventChannel<T>> Channel<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = ManualResetValueTaskCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(eventSelector, completionSource));
    }

    partial class Arguments
    {
        public readonly struct ObserveArgument<T> : ICallableArgument
        {
            private readonly Func<IReadOnlyEventBroker, IObservableEvent<T>> _eventSelector;
            private readonly ManualResetValueTaskCompletionSource<EventChannel<T>> _completionSource;

            public Func<IReadOnlyEventBroker, IObservableEvent<T>> EventSelector => _eventSelector;

            internal ObserveArgument(
                Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector,
                ManualResetValueTaskCompletionSource<EventChannel<T>> completionSource)
            {
                _eventSelector = eventSelector;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext coroutineContext)
            {
                var eventBroker = coroutineContext.GetBequestedEventBroker(ServiceKeys.EventBrokerKey);
                var emissions = System.Threading.Channels.Channel.CreateUnbounded<T>();
                EventSelector(eventBroker).Subscribe((emission, writer) => writer.TryWrite(emission), emissions.Writer);
                _completionSource.SetResult(new EventChannel<T>(emissions));
            }
        }
    }
}
