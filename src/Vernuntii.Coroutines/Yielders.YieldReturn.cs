using System.Runtime.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<T>(T value, ManualResetValueTaskCompletionSource<T> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new YieldReturnArgument<T>(value);
        argumentReceiver.ReceiveCallableArgument(in YieldReturnKey, in argument, completionSource);
    }
}

partial class Yielders
{
    public static Coroutine YieldReturn<T>(T value)
    {
        var completionSource = ManualResetValueTaskCompletionSource<T>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor<T>(value, completionSource));
    }

    partial class Arguments
    {
        [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly struct YieldReturnArgument<T>(T result) : ICallableArgument
        {
            public T Result {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => result;
            }

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource) => 
                Unsafe.As<ManualResetValueTaskCompletionSource<T>>(completionSource).SetResult(Result);
        }
    }
}
