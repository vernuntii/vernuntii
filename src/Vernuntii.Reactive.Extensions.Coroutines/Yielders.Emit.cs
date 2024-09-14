using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(IEventDiscriminator<T> eventDiscriminator, T eventData, ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new EmitArgument<T>(eventDiscriminator, eventData, completionSource);
        argumentReceiver.ReceiveCallableArgument(in EmitKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine Emit<T>(IEventDiscriminator<T> eventDiscriminator, T eventData)
    {
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor<T>(eventDiscriminator, eventData, completionSource));
    }

    partial class Arguments
    {
        public readonly struct EmitArgument<T> : ICallableArgument
        {
            private readonly IEventDiscriminator<T> _eventDiscriminator;
            private readonly T _eventData;
            private readonly ManualResetValueTaskCompletionSource<Nothing> _completionSource;

            public IEventDiscriminator<T> EventDiscriminator => _eventDiscriminator;
            public T EventData => _eventData;

            internal EmitArgument(
                IEventDiscriminator<T> eventDiscriminator,
                T eventData,
                ManualResetValueTaskCompletionSource<Nothing> completionSource)
            {
                _eventDiscriminator = eventDiscriminator;
                _eventData = eventData;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext coroutineContext)
            {
                var eventBroker = coroutineContext.GetBequestedEventBroker(ServiceKeys.EventBrokerKey);
                eventBroker.EmitAsync(EventDiscriminator, EventData).DelegateCompletion(_completionSource);
            }
        }
    }
}
