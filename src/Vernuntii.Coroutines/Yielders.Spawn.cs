using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.CompilerServices;
using static Vernuntii.Coroutines.Yielders.Arguments;

namespace Vernuntii.Coroutines;

file class CoroutineArgumentReceiverAcceptor<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<Coroutine> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new SpawnArgument<TClosure>(provider, closure, isProviderWithClosure, completionSource);
        argumentReceiver.ReceiveCallableArgument(in SpawnKey, in argument, completionSource);
    }
}

file class CoroutineArgumentReceiverAcceptor<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure, ManualResetCoroutineCompletionSource<Coroutine<TResult>> completionSource) : AbstractCoroutineArgumentReceiverAcceptor
{
    protected override void AcceptCoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
    {
        var argument = new SpawnArgument<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource);
        argumentReceiver.ReceiveCallableArgument(in SpawnKey, in argument, completionSource);
    }
}

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> SpawnInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Coroutine>.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure>(provider, closure, isProviderWithClosure, completionSource));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> SpawnInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Coroutine<TResult>>.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(completionSource.CreateGenericValueTask(), new CoroutineArgumentReceiverAcceptor<TClosure, TResult>(provider, closure, isProviderWithClosure, completionSource));
    }

    public static Coroutine<Coroutine> Spawn(Func<Coroutine> provider) => SpawnInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<Coroutine> Spawn<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => SpawnInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<Coroutine<TResult>> Spawn<TResult>(Func<Coroutine<TResult>> provider) => SpawnInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<Coroutine<TResult>> Spawn<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => SpawnInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct SpawnArgument<TClosure> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<Coroutine> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal SpawnArgument(
                Delegate provider,
                TClosure closure,
                bool isProviderWithClosure,
                ManualResetCoroutineCompletionSource<Coroutine> completionSource)
            {
                _provider = provider;
                _closure = closure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                if (coroutineAwaiter.IsCompleted) {
                    _completionSource.SetResult(coroutine);
                    return;
                }

                ref var contextToBequest = ref _completionSource._coroutineContext;
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
                childCoroutine.MarkCoroutineAsHandled();
                _completionSource.SetResult(childCoroutine);
            }
        }

        internal readonly struct SpawnArgument<TClosure, TResult> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<Coroutine<TResult>> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal SpawnArgument(
                Delegate provider,
                TClosure closure,
                bool isProviderWithClosure,
                ManualResetCoroutineCompletionSource<Coroutine<TResult>> completionSource)
            {
                _provider = provider;
                _closure = closure;
                _isProviderWithClosure = isProviderWithClosure;
                _completionSource = completionSource;
            }

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (_isProviderWithClosure) {
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(Provider)(Closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(Provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                if (coroutineAwaiter.IsCompleted) {
                    _completionSource.SetResult(coroutine);
                    return;
                }

                ref var contextToBequest = ref _completionSource._coroutineContext;
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
                childCoroutine.MarkCoroutineAsHandled();
                _completionSource.SetResult(childCoroutine);
            }

        }
    }
}
