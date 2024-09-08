using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Yielders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> LaunchInternal<TClosure>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Coroutine>.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<TClosure>(provider, closure, isProviderWithClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.LaunchKey, in argument, completionSource);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> LaunchInternal<TClosure, TResult>(Delegate provider, TClosure closure, bool isProviderWithClosure)
    {
        var completionSource = ManualResetCoroutineCompletionSource<Coroutine<TResult>>.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(completionSource.CreateGenericValueTask(), CoroutineArgumentReceiver);

        void CoroutineArgumentReceiver(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<TClosure,TResult>(provider, closure, isProviderWithClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.LaunchKey, in argument, completionSource);
        }
    }

    public static Coroutine<Coroutine> Launch(Func<Coroutine> provider) => LaunchInternal<object?>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<Coroutine> Launch<TClosure>(Func<TClosure, Coroutine> provider, TClosure closure) => LaunchInternal(provider, closure, isProviderWithClosure: true);

    public static Coroutine<Coroutine<TResult>> Launch<TResult>(Func<Coroutine<TResult>> provider) => LaunchInternal<object?, TResult>(provider, closure: null, isProviderWithClosure: false);

    public static Coroutine<Coroutine<TResult>> Launch<TClosure, TResult>(Func<TClosure, Coroutine<TResult>> provider, TClosure closure) => LaunchInternal<TClosure, TResult>(provider, closure, isProviderWithClosure: true);

    partial class Arguments
    {
        public readonly struct LaunchArgument<TClosure> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<Coroutine> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal LaunchArgument(
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
                    coroutine = Unsafe.As<Func<TClosure, Coroutine>>(_provider)(_closure!);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<object?>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();

                ref var contextToBequest = ref _completionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                context.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
                    try {
                        coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsHandled();
                _completionSource.SetResult(coroutine);
            }
        }

        internal readonly struct LaunchArgument<TClosure, TResult> : ICallableArgument
        {
            private readonly Delegate _provider;
            private readonly TClosure _closure;
            private readonly bool _isProviderWithClosure;
            private readonly ManualResetCoroutineCompletionSource<Coroutine<TResult>> _completionSource;

            public readonly Delegate Provider => _provider;
            public readonly TClosure Closure => _closure;

            internal LaunchArgument(
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
                    coroutine = Unsafe.As<Func<TClosure, Coroutine<TResult>>>(_provider)(_closure);
                } else {
                    coroutine = Unsafe.As<Func<Coroutine<TResult>>>(_provider)();
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ManualResetCoroutineCompletionSource<TResult>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateGenericValueTask();

                ref var contextToBequest = ref _completionSource._coroutineContext;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                context.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
                    try {
                        var result = coroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutine.MarkCoroutineAsHandled();
                _completionSource.SetResult(coroutine);
            }
        }
    }
}
