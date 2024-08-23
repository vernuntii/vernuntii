using System.Runtime.CompilerServices;
using System.Text;
using Vernuntii.Coroutines.v1;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine CallInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = Coroutine<object?>.CompletionSource.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.CallArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> CallInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = Coroutine<TResult>.CompletionSource.RentFromCache();
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument<TResult>(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.CallArgumentType);
        }
    }

    public static Coroutine Call(Func<Coroutine> provider) => CallInternal(provider, providerClosure: null);

    public static Coroutine<TResult> Call<TResult>(Func<Coroutine<TResult>> provider) => CallInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal readonly static Key CallArgumentType = new(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("call"));

        internal struct CallArgument(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<object?>.CompletionSource completionSource) : ICallbackArgument
        {
            private readonly Coroutine<object?>.CompletionSource _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeClosured<Coroutine>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var completionSource = _completionSource;
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineContext);
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

        internal struct CallArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<TResult>.CompletionSource completionSource) : ICallbackArgument
        {
            private readonly Coroutine<TResult>.CompletionSource _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeClosured<Coroutine<TResult>>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var completionSource = _completionSource;
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineContext);
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
