using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<Nothing> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new CallArgument<TClosure>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in CallKey, in argument, completionSource);
    }
}

file class CoroutineArgumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<TResult> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new CallArgument<TClosure, TResult>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in CallKey, in argument, completionSource);
    }
}

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine CallInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
        return new Coroutine(completionSource.CreateValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<TResult> CallInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
        return new Coroutine<TResult>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine Call(Func<Coroutine> provider) => CallInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine Call<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => CallInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<TResult> Call<TResult>(Func<Coroutine<TResult>> provider) => CallInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<TResult> Call<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => CallInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct CallArgument<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
        {
            public Delegate Provider {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => provider;
            }

            public TClosure Closure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => closure;
            }

            public bool IsProviderWithClosure {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => isProviderWithClosure;
            }

            readonly void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<Nothing>>(completionSource);

                var contextToBequest = default(CoroutineContext);
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, in contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(typedCompletionSource);
            }
        }

        public readonly struct CallArgument<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
        {
            public Delegate Provider { get; } = provider;
            public TClosure Closure { get; } = closure;
            public bool IsProviderWithClosure { get; } = isProviderWithClosure;

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine<TResult> coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<TResult>>(completionSource);

                var contextToBequest = default(CoroutineContext);
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, in contextToBequest);
                coroutineAwaiter.DelegateCoroutineCompletion(typedCompletionSource);
            }
        }
    }
}
