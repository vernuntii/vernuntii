using System.Text;
using Vernuntii.Coroutines;
using Vernuntii.Coroutines.v1;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine<EventChannel<T>> Channel<T>(this Effect _, Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = new TaskCompletionSource<EventChannel<T>>();
        return new Coroutine<EventChannel<T>>(completionSource.Task, ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.ObserveArgument<T>(eventSelector, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.ObserveArgumentType);
        }
    }

    partial class Arguments
    {
        internal readonly static Key ObserveArgumentType = new Key(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("observe"));

        internal readonly struct ObserveArgument<T>(
            Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector,
            TaskCompletionSource<EventChannel<T>> completionSource) : ICallbackArgument
        {
            public void Callback(ref CoroutineContext coroutineContext) => throw new NotImplementedException();
        }
    }
}
