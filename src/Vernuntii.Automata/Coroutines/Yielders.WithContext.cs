using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine WithContextInternal<TClosure>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ValueTaskCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument<TClosure>(additiveContext, provider, closure, isProviderWithClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_withContextArgumentType, in argument, completionSource);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> WithContextInternal<TClosure, TResult>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.WithContextArgument<TClosure, TResult>(additiveContext, provider, closure, isProviderWithClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_withContextArgumentType, in argument, completionSource);
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
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(_provider)(_closure!);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _additiveContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCompletion(_completionSource);
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
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(_provider)(_closure!);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _additiveContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCompletion(_completionSource);
            }
        }
    }
}
