using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<CoroutineAwaitable> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new SpawnArgument<TClosure>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in SpawnKey, in argument, completionSource);
    }
}

file class CoroutineArgumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new SpawnArgument<TClosure, TResult>(provider, closure, isProviderWithClosure);
        argumentReceiver.ReceiveCallableArgument(in SpawnKey, in argument, completionSource);
    }
}

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<CoroutineAwaitable> SpawnInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<CoroutineAwaitable>.RentFromCache();
        return new Coroutine<CoroutineAwaitable>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<CoroutineAwaitable<TResult>> SpawnInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>>.RentFromCache();
        return new Coroutine<CoroutineAwaitable<TResult>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine<CoroutineAwaitable> Spawn(Func<Coroutine> provider) => SpawnInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<CoroutineAwaitable> Spawn<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => SpawnInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<CoroutineAwaitable<TResult>> Spawn<TResult>(Func<Coroutine<TResult>> provider) => SpawnInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<CoroutineAwaitable<TResult>> Spawn<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => SpawnInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct SpawnArgument<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
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
                    coroutine = Unsafe.As<Func<CoroutineAwaitable>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<CoroutineAwaitable>>(completionSource);

                ref var contextToBequest = ref typedCompletionSource._coroutineContext;
                contextToBequest.TreatAsNewChild();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                Coroutine childCoroutine;
                if (coroutine._coroutineAction != CoroutineAction.Child) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutineAwaiter, ref contextToBequest);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();

                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<Nothing>.RentFromCache();
                childCoroutine._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.ActOnCoroutine(ref childCoroutineAwaiter, ref contextToBequest);
                childCoroutineAwaiter.DelegateCoroutineCompletion(intermediateCompletionSource);
                childCoroutine.MarkCoroutineAsActedOn();
                typedCompletionSource.SetResult(new(in childCoroutine));
            }
        }

        internal readonly struct SpawnArgument<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure) : ICallableArgument
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
                    coroutine = Unsafe.As<Func<CoroutineAwaitable<TResult>>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();

                var typedCompletionSource = Unsafe.As<ManualResetCoroutineCompletionSource<CoroutineAwaitable<TResult>>>(completionSource);

                ref var contextToBequest = ref typedCompletionSource._coroutineContext;
                contextToBequest.TreatAsNewChild();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                Coroutine<TResult> childCoroutine;
                if (coroutine._coroutineAction != CoroutineAction.Child) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine<ConfiguredCoroutineAwaitable<TResult>.ConfiguredCoroutineAwaiter, TResult>(ref coroutineAwaiter, ref contextToBequest);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();

                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
                childCoroutine._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.ActOnCoroutine(ref childCoroutineAwaiter, ref contextToBequest);
                childCoroutineAwaiter.DelegateCoroutineCompletion(intermediateCompletionSource);
                childCoroutine.MarkCoroutineAsActedOn();
                typedCompletionSource.SetResult(new(in childCoroutine));
            }

        }
    }
}
