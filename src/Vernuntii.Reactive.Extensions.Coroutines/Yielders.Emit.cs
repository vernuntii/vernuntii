using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using static Vernuntii.Reactive.Extensions.Coroutines.Yielders.Arguments;

namespace Vernuntii.Reactive.Extensions.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(IEventDiscriminator<T> eventDiscriminator, T eventData, ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new EmitArgument<T>(eventDiscriminator, eventData);
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
        public readonly struct EmitArgument<T>(IEventDiscriminator<T> eventDiscriminator, T eventData) : ICallableArgument
        {
            public IEventDiscriminator<T> EventDiscriminator {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eventDiscriminator;
            }

            public T EventData {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => eventData;
            }

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                var eventBroker = context.GetBequestedEventBroker(ServiceKeys.EventBrokerKey);
                eventBroker.EmitAsync(EventDiscriminator, EventData).DelegateCompletion(Unsafe.As<ManualResetValueTaskCompletionSource<Nothing>>(completionSource));
            }
        }
    }
}
