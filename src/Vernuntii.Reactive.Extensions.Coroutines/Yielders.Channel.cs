using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector, ManualResetValueTaskCompletionSource<EventChannel<T>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
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
        var completionSource = ManualResetValueTaskCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<T>(eventSelector, completionSource));
    }

    partial class Arguments
    {
        public readonly struct ObserveArgument<T>(Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector) : ICallableArgument
        {
            public Func<IReadOnlyEventBroker, IObservableEvent<T>> EventSelector {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eventSelector;
            }

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                var eventBroker = context.GetBequestedEventBroker(ServiceKeys.EventBrokerKey);
                var emissions = System.Threading.Channels.Channel.CreateUnbounded<T>();
                EventSelector(eventBroker).Subscribe((emission, writer) => writer.TryWrite(emission), emissions.Writer);
                Unsafe.As<ManualResetValueTaskCompletionSource<EventChannel<T>>>(completionSource).SetResult(new EventChannel<T>(emissions));
            }
        }
    }
}
