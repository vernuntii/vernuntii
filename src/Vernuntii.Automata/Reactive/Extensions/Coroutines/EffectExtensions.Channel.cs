using Vernuntii.Coroutines;
using Vernuntii.Coroutines.Iterators;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine<EventChannel<T>> Channel<T>(this Effect _, Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = ValueTaskCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.ObserveArgument<T>(eventSelector, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.ObserveArgumentType, in argument, completionSource);
        }
    }

    partial class Arguments
    {
        internal readonly struct ObserveArgument<T>(
            Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector,
            ValueTaskCompletionSource<EventChannel<T>> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<EventChannel<T>> _completionSource = completionSource;

            void ICallableArgument.Callback(in CoroutineContext coroutineContext) => throw new NotImplementedException();
        }
    }
}
