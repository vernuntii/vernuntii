using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    public static Coroutine Emit<T>(this Yielders _, IEventDiscriminator<T> eventDiscriminator, T eventData)
    {
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.EmitArgument<T>(eventDiscriminator, eventData, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.EmitKey, in argument, completionSource);
        }
    }

    partial class Arguments
    {
        internal readonly struct EmitArgument<T> : ICallableArgument
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
