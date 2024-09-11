using System.Runtime.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineargumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new WithContextArgument<TClosure>(provider, closure, isProviderWithClosure, completionSource);
        argumentReceiver.ReceiveCallableArgument(in WithContextKey, in argument, completionSource);
    }
}

file class CoroutineargumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<TResult> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new WithContextArgument<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource);
        argumentReceiver.ReceiveCallableArgument(in WithContextKey, in argument, completionSource);
    }
}

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine WithContextInternal<TClosure>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
        completionSource._coroutineContext = additiveContext;
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineargumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> WithContextInternal<TClosure, TResult>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
        completionSource._coroutineContext = additiveContext;
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), new CoroutineargumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine WithContext(CoroutineContext additiveContext, Func<Coroutine> provider) => WithContextInternal<object?>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine WithContext<TClosure>(CoroutineContext additiveContext, Func<TClosure, Coroutine> provider, TClosure closure) => WithContextInternal(additiveContext, provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> WithContext<TResult>(CoroutineContext additiveContext, Func<Coroutine<TResult>> provider) => WithContextInternal<object?, TResult>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<TResult> WithContext<TClosure, TResult>(CoroutineContext additiveContext, Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => WithContextInternal<TClosure, TResult>(additiveContext, provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct WithContextArgument<TClosure> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure? _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<Nothing> _completionSource;

            internal WithContextArgument(
                Delegate provider,
                TClosure? providerClosure,
                bool isProviderWithClosure,
                ManualResetCoroutineCompletionSource<Nothing> completionSource)
            {
                _provider = provider;
                _closure = providerClosure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine
                    coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(_provider)(_closure!);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _completionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(_completionSource);
            }
        }

        public readonly struct WithContextArgument<TClosure, TResult> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure? _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<TResult> _completionSource;

            internal WithContextArgument(
                Delegate provider,
                TClosure? providerClosure,
                bool isProviderWithClosure,
                ManualResetCoroutineCompletionSource<TResult> completionSource)
            {
                _provider = provider;
                _closure = providerClosure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            readonly void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(_provider)(_closure!);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                ref var contextToBequest = ref _completionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(_completionSource);
            }
        }
    }
}
