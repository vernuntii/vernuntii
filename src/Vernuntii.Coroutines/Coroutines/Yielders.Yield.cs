using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Yielders
{
    public static Coroutine Yield()
    {
        var completionSource = ManualResetValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.YieldArgument(completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.YieldKey, in argument, completionSource);
        }
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
