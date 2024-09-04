using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine CallInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
       var completionSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        CoroutineArgumentReceiverDelegateWithClosure<Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>> argumentReceiver = AcceptArgumentReceiver;
        var argumentReceiverClosure = CoroutineArgumentReceiverDelegateClosure.Create(provider, closure, isProviderWithClosure, completionSource, argumentReceiver);
        return new Coroutine(completionSource.CreateValueTask(), argumentReceiverClosure.CoroutineArgumentReceiver);

        static void AcceptArgumentReceiver(
            Tuple<Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>, CoroutineArgumentReceiverDelegateWithClosure<Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>>> closure,
            ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument<TClosure>(closure.Item1, closure.Item2, closure.Item3, closure.Item4);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, closure.Item4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> CallInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
        CoroutineArgumentReceiverDelegateWithClosure<Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>> argumentReceiver = AcceptArgumentReceiver;
        var argumentReceiverClosure = CoroutineArgumentReceiverDelegateClosure.Create(provider, closure, isProviderWithClosure, completionSource, argumentReceiver);
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverClosure.CoroutineArgumentReceiver);

        static void AcceptArgumentReceiver(
            Tuple<Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>, CoroutineArgumentReceiverDelegateWithClosure<Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>>> closure,
            ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.CallArgument<TClosure, TResult>(closure.Item1, closure.Item2, closure.Item3, closure.Item4);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, closure.Item4);
        }
    }

    public static Coroutine Call(Func<Coroutine> provider) => CallInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine Call<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => CallInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> Call<TResult>(Func<Coroutine<TResult>> provider) => CallInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<TResult> Call<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => CallInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct CallArgument<TClosure> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ValueTaskCompletionSource<Nothing> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal CallArgument(
                Delegate provider,
                TClosure closure,
                bool isProviderWithClosure,
                ValueTaskCompletionSource<Nothing> completionSource)
            {
                _provider = provider;
                _closure = closure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(_provider)(_closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCompletion(_completionSource);
            }
        }

        public readonly struct CallArgument<TClosure, TResult> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ValueTaskCompletionSource<TResult> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal CallArgument(
                Delegate provider,
                TClosure closure,
                bool isProviderWithClosure,
                ValueTaskCompletionSource<TResult> completionSource)
            {
                _provider = provider;
                _closure = closure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(_provider)(_closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(_provider)();
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
