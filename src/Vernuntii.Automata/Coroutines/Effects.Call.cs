using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Coroutines;

partial class Effects
{
    internal readonly static ArgumentType CallArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("call"));
    
    public static Coroutine CallAsync(Func<Coroutine> provider)
    {
        var completionSource = Coroutine<object?>.CompletionSource.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new CallArgument(provider, completionSource);
            argumentReceiver.ReceiveArgument(in argument, in CallArgumentType);
        }
    }

    public static Coroutine<T> CallAsync<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = Coroutine<T>.CompletionSource.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new CallArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveArgument(in argument, in CallArgumentType);
        }
    }

    internal struct CallArgument(Func<Coroutine> provider, Coroutine<object?>.CompletionSource completionSource) : ICallbackArgument
    {
        private readonly Coroutine<object?>.CompletionSource _completionSource = completionSource;

        void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode) {
            var coroutine = provider();
            var coroutineAwaiter = coroutine.GetAwaiter();
            var completionSource = _completionSource;
            coroutineAwaiter.UnsafeOnCompleted(() => {
                try {
                    coroutineAwaiter.GetResult();
                    completionSource.SetResult(default);
                } catch (Exception error) {
                    completionSource.SetException(error);
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
        }
    }

    internal struct CallArgument<T>(Func<Coroutine<T>> provider, Coroutine<T>.CompletionSource completionSource) : ICallbackArgument
    {
        private readonly Coroutine<T>.CompletionSource _completionSource = completionSource;

        void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode) {
            var coroutine = provider();
            var coroutineAwaiter = coroutine.GetAwaiter();
            var completionSource = _completionSource;
            coroutineAwaiter.UnsafeOnCompleted(() => {
                try {
                    var result = coroutineAwaiter.GetResult();
                    completionSource.SetResult(result);
                } catch (Exception error) {
                    completionSource.SetException(error);
                }
            });
            coroutineAwaiter.PropagateCoroutineNode(ref coroutineNode);
            coroutineAwaiter.StartStateMachine();
        }
    }
}
