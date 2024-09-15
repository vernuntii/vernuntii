using Vernuntii.Coroutines.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<CoroutineAwaitable> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new LaunchArgument<TClosure>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in LaunchKey, in argument, completionSource);
    }
}

file class CoroutineArgumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new LaunchArgument<TClosure, TResult>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in LaunchKey, in argument, completionSource);
    }
}

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<CoroutineAwaitable> LaunchInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<CoroutineAwaitable>.RentFromCache();
        return new Coroutine<CoroutineAwaitable>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<CoroutineAwaitable<TResult>> LaunchInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>>.RentFromCache();
        return new Coroutine<CoroutineAwaitable<TResult>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine<CoroutineAwaitable> Launch(Func<Coroutine> provider) => LaunchInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<CoroutineAwaitable> Launch<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => LaunchInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<CoroutineAwaitable<TResult>> Launch<TResult>(Func<Coroutine<TResult>> provider) => LaunchInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<CoroutineAwaitable<TResult>> Launch<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => LaunchInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct LaunchArgument<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
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

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<object?>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<CoroutineAwaitable>>(completionSource);

                var contextToBequest = default(CoroutineContext);
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, in contextToBequest);
                context.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
                    try {
                        coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsActedOn();
                typedCompletionSource.SetResult(new(in coroutine));
            }
        }

        internal readonly struct LaunchArgument<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
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

            void ICallableArgument.Callback<TCompletionSource>(in CoroutineContext context, TCompletionSource completionSource)
            {
                Coroutine<TResult> coroutine;
                if (IsProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateGenericValueTask();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>>>(completionSource);

                var contextToBequest = default(CoroutineContext);
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.ActOnCoroutine(ref coroutineAwaiter, in contextToBequest);
                context.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
                    try {
                        var result = coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsActedOn();
                typedCompletionSource.SetResult(new(in coroutine));
            }
        }
    }
}
