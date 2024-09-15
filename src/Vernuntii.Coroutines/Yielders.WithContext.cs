using System.Runtime.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new WithContextArgument<TClosure>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in WithContextKey, in argument, completionSource);
    }
}

file class CoroutineArgumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<TResult> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new WithContextArgument<TClosure, TResult>(provider, closure, isProviderWithClosure);
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
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> WithContextInternal<TClosure, TResult>(CoroutineContext additiveContext, Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
        completionSource._coroutineContext = additiveContext;
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine WithContext(CoroutineContext additiveContext, Func<Coroutine> provider) => WithContextInternal<object?>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine WithContext<TClosure>(CoroutineContext additiveContext, Func<TClosure, Coroutine> provider, TClosure closure) => WithContextInternal(additiveContext, provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> WithContext<TResult>(CoroutineContext additiveContext, Func<Coroutine<TResult>> provider) => WithContextInternal<object?, TResult>(additiveContext, provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<TResult> WithContext<TClosure, TResult>(CoroutineContext additiveContext, Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => WithContextInternal<TClosure, TResult>(additiveContext, provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct WithContextArgument<TClosure>(Delegate provider, TClosure providerClosure, bool isProviderWithClosure) : ICallableArgument
        {
            public Delegate Provider {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => provider;
            }

            public TClosure ProviderClosure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => providerClosure;
            }

            public bool IsProviderWithClosure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => isProviderWithClosure;
            }

            readonly void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine
                    coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(Provider)(ProviderClosure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<Nothing>>(completionSource);

                ref var contextToBequest = ref typedCompletionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(typedCompletionSource);
            }
        }

        public readonly struct WithContextArgument<TClosure, TResult>(Delegate provider, TClosure providerClosure, bool isProviderWithClosure) : ICallableArgument
        {
            public Delegate Provider {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => provider;
            }

            public TClosure ProviderClosure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => providerClosure;
            }

            public bool IsProviderWithClosure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => isProviderWithClosure;
            }

            readonly void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine<TResult> coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(Provider)(ProviderClosure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<TResult>>(completionSource);

                ref var contextToBequest = ref typedCompletionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling(additionalBequesterOrigin: CoroutineContextBequesterOrigin.ContextBequester);
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, ref contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(typedCompletionSource);
            }
        }
    }
}
