using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    public static Coroutine<EventChannel<T>> Channel<T>(this Yielders _, Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = ManualResetValueTaskCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.ObserveArgument<T>(eventSelector, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.ObserveKey, in argument, completionSource);
        }
    }

    partial class Arguments
    {
        internal readonly struct ObserveArgument<T>(
            Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector,
            ManualResetValueTaskCompletionSource<EventChannel<T>> completionSource) : ICallableArgument
        {
            private readonly ManualResetValueTaskCompletionSource<EventChannel<T>> _completionSource = completionSource;

            void ICallableArgument.Callback(in CoroutineContext coroutineContext) => throw new NotImplementedException();
        }
    }
}
