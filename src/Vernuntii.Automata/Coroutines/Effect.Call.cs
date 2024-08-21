using System.Text;

namespace Vernuntii.Coroutines;

partial class Effect
{
    public static Coroutine Call(Func<Coroutine> provider)
    {
        var completionSource = Coroutine<object?>.CompletionSource.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.CallArgumentType);
        }
    }

    public static Coroutine CallTask(Func<Task> provider)
    {
        return Call(() => new Coroutine(new ValueTask(provider())));
    }

    public static Coroutine CallValueTask(Func<ValueTask> provider)
    {
        return Call(() => new Coroutine(provider()));
    }

    public static Coroutine<T> Call<T>(Func<Coroutine<T>> provider)
    {
        var completionSource = Coroutine<T>.CompletionSource.RentFromCache();
        return new Coroutine<T>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument<T>(provider, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.CallArgumentType);
        }
    }

    public static Coroutine<T> CallTask<T>(Func<Task<T>> provider)
    {
        return Call(() => new Coroutine<T>(new ValueTask<T>(provider())));
    }

    public static Coroutine<T> CallValueTask<T>(Func<ValueTask<T>> provider)
    {
        return Call(() => new Coroutine<T>(provider()));
    }

    partial class Arguments
    {
        internal readonly static ArgumentType CallArgumentType = new ArgumentType(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("call"));

        internal struct CallArgument(Func<Coroutine> provider, Coroutine<object?>.CompletionSource completionSource) : ICallbackArgument
        {
            private readonly Coroutine<object?>.CompletionSource _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
            {
                var coroutine = provider();
                var coroutineAwaiter = coroutine.GetAwaiter();
                var completionSource = _completionSource;
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineNode);
                coroutineAwaiter.UnsafeOnCompleted(() => {
                    try {
                        coroutineAwaiter.GetResult();
                        completionSource.SetResult(default);
                    } catch (Exception error) {
                        completionSource.SetException(error);
                    }
                });
            }
        }

        internal struct CallArgument<T>(Func<Coroutine<T>> provider, Coroutine<T>.CompletionSource completionSource) : ICallbackArgument
        {
            private readonly Coroutine<T>.CompletionSource _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineStackNode coroutineNode)
            {
                var coroutine = provider();
                var coroutineAwaiter = coroutine.GetAwaiter();
                var completionSource = _completionSource;
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineNode);
                coroutineAwaiter.UnsafeOnCompleted(() => {
                    try {
                        var result = coroutineAwaiter.GetResult();
                        completionSource.SetResult(result);
                    } catch (Exception error) {
                        completionSource.SetException(error);
                    }
                });
            }
        }
    }
}
