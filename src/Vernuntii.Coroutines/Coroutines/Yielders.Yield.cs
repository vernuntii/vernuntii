using System.Runtime.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineargumentReceiverAcceptor(ManualResetValueTaskCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new YieldArgument(completionSource);
        argumentReceiver.ReceiveCallableArgument(in YieldKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine Yield()
    {
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineargumentReceiverAcceptor(completionSource));
    }

    partial class Arguments
    {
        internal readonly struct YieldArgument : ICallableArgument
        {
            private readonly ManualResetValueTaskCompletionSource<Nothing> _completionSource;

            internal YieldArgument(ManualResetValueTaskCompletionSource<Nothing> completionSource) => _completionSource = completionSource;

            void ICallableArgument.Callback(in CoroutineContext context) => new YieldAwaitable.YieldAwaiter().UnsafeOnCompleted(_completionSource.SetDefaultResult);
        }
    }
}
