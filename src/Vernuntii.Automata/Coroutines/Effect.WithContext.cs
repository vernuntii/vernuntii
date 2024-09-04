using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine WithContextInternal<TClosure>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        CoroutineArgumentReceiverDelegateWithClosure<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>> argumentReceiver = AcceptArgumentReceiver;
        var argumentReceiverClosure = CoroutineArgumentReceiverDelegateClosure.Create(additiveContext, provider, closure, isProviderWithClosure, completionSource, argumentReceiver);
        return new Coroutine(completionSource.CreateValueTask(), argumentReceiverClosure.CoroutineArgumentReceiver);

        static void AcceptArgumentReceiver(
            Tuple<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>, CoroutineArgumentReceiverDelegateWithClosure<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<Nothing>>> closure,
            ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument<TClosure>(closure.Item1, closure.Item2, closure.Item3, closure.Item4, closure.Item5);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, closure.Item5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> WithContextInternal<TClosure, TResult>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
        CoroutineArgumentReceiverDelegateWithClosure<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>> argumentReceiver = AcceptArgumentReceiver;
        var argumentReceiverClosure = CoroutineArgumentReceiverDelegateClosure.Create(additiveContext, provider, closure, isProviderWithClosure, completionSource, argumentReceiver);
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), argumentReceiverClosure.CoroutineArgumentReceiver);

        static void AcceptArgumentReceiver(
            Tuple<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>, CoroutineArgumentReceiverDelegateWithClosure<CoroutineContext, Delegate, TClosure, bool, ValueTaskCompletionSource<TResult>>> closure,
            ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument<TClosure, TResult>(closure.Item1, closure.Item2, closure.Item3, closure.Item4, closure.Item5);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_callArgumentType, in argument, closure.Item5);
        }
    }

    public static Coroutine WithContext(CoroutineContext additiveContext, Func<Coroutine> provider) => WithContextInternal<object?>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine WithContext<TClosure>(CoroutineContext additiveContext, Func<TClosure, Coroutine> provider, TClosure closure) => WithContextInternal(additiveContext, provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> WithContext<TResult>(CoroutineContext additiveContext, Func<Coroutine<TResult>> provider) => WithContextInternal<object?, TResult>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<TResult> WithContext<TClosure, TResult>(CoroutineContext additiveContext, Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => WithContextInternal<TClosure, TResult>(additiveContext, provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public struct WithContextArgument<TClosure> : ICallableArgument
        {
            private CoroutineContext _additiveContext;
            private readonly Delegate _provider;
            private readonly TClosure? _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ValueTaskCompletionSource<Nothing> _completionSource;

            internal WithContextArgument(
                CoroutineContext additiveContext,
                Delegate provider,
                TClosure? providerClosure,
                bool isProviderWithClosure,
                ValueTaskCompletionSource<Nothing> completionSource)
            {
                _additiveContext = additiveContext;
                _provider = provider;
                _closure = providerClosure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine
                    coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(_provider)(_closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _additiveContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                var completionSource = _completionSource;
                coroutineAwaiter.DelegateCompletion(completionSource);
            }
        }

        public struct WithContextArgument<TClosure, TResult> : ICallableArgument
        {
            private CoroutineContext _additiveContext;
            private readonly Delegate _provider;
            private readonly TClosure? _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ValueTaskCompletionSource<TResult> _completionSource;

            internal WithContextArgument(
                CoroutineContext additiveContext,
                Delegate provider,
                TClosure? providerClosure,
                bool isProviderWithClosure,
                ValueTaskCompletionSource<TResult> completionSource)
            {
                _additiveContext = additiveContext;
                _provider = provider;
                _closure = providerClosure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(_provider)(_closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _additiveContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                var completionSource = _completionSource;
                coroutineAwaiter.DelegateCompletion(completionSource);
            }
        }
    }
}
