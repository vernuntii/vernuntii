using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine CallInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<object?>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in argument, in Arguments.s_callArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> CallInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument<TResult>(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in argument, in Arguments.s_callArgumentType);
        }
    }

    public static Coroutine Call(Func<Coroutine> provider) => CallInternal(provider, providerClosure: null);

    public static Coroutine<TResult> Call<TResult>(Func<Coroutine<TResult>> provider) => CallInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal struct CallArgument(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<object?> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<object?> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallableArgument.CompletionSource => _completionSource;

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var completionSource = _completionSource;
                var contextToBequest = context;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritirBequestCoroutineContext(ref contextToBequest, in context);
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
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
            ValueTaskCompletionSource<TResult> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<TResult> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallableArgument.CompletionSource => _completionSource;

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var completionSource = _completionSource;
                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritirBequestCoroutineContext(ref contextToBequest, in context);
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
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
