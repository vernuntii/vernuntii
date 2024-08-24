using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine WithContextInternal(CoroutineContext additiveContext, Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<object?>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument(additiveContext, provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.s_withContextArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> WithContextInternal<TResult>(CoroutineContext additiveContext, Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument<TResult>(additiveContext, provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.s_withContextArgumentType);
        }
    }

    public static Coroutine WithContext(CoroutineContext additiveContext, Func<Coroutine> provider) => WithContextInternal(additiveContext, provider, providerClosure: null);

    public static Coroutine<TResult> WithContext<TResult>(CoroutineContext additiveContext, Func<Coroutine<TResult>> provider) => WithContextInternal<TResult>(additiveContext, provider, providerClosure: null);

    partial class Arguments
    {
        internal struct WithContextArgument(
            CoroutineContext additiveContext,
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<object?> completionSource) : ICallbackArgument
        {
            private readonly ValueTaskCompletionSource<object?> _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext parentContext)
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
                var contextAsMerged = new CoroutineContextBequester(parentContext, additiveContext, CoroutineContextBequeathBehaviour.PrivateBequestingUntilChild);
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextAsMerged);
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

        internal struct WithContextArgument<TResult>(
            CoroutineContext additiveContext,
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<TResult> completionSource) : ICallbackArgument
        {
            private readonly ValueTaskCompletionSource<TResult> _completionSource = completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext parentContext)
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
                var contextAsMerge = new CoroutineContextBequester(parentContext, additiveContext, CoroutineContextBequeathBehaviour.PrivateBequestingUntilChild);
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextAsMerge);
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
