using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine CallInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, completionSource);
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
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, completionSource);
        }
    }

    public static Coroutine Call(Func<Coroutine> provider) => CallInternal(provider, providerClosure: null);

    public static Coroutine<TResult> Call<TResult>(Func<Coroutine<TResult>> provider) => CallInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal struct CallArgument(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Nothing> completionSource) : ICallableArgument
        {
            public Delegate Provider { get; } = provider;
            public IClosure? ProviderClosure { get; } = providerClosure;

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (ProviderClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(Provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = ProviderClosure.InvokeDelegateWithClosure<Coroutine>(Provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCompletion(completionSource);
            }
        }

        public struct CallArgument<TResult> : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<TResult> _completionSource;

            public Delegate Provider { get; }
            public IClosure? ProviderClosure { get; }

            internal CallArgument(
                Delegate provider,
                IClosure? providerClosure,
                ValueTaskCompletionSource<TResult> completionSource)
            {
                _completionSource = completionSource;
                Provider = provider;
                ProviderClosure = providerClosure;
            }

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (ProviderClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(Provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = ProviderClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(Provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCompletion(_completionSource);
            }
        }
    }
}
