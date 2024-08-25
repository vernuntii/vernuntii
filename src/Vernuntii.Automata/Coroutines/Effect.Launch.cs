using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> LaunchInternal(Delegate provider, IClosure? providerClosure)
    {
        var immediateCompletionSource = ValueTaskCompletionSource<Coroutine>.RentFromCache();
        return new Coroutine<Coroutine>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument(provider, providerClosure, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.s_launchArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> LaunchInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var immediateCompletionSource = ValueTaskCompletionSource<Coroutine<TResult>>.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<TResult>(provider, providerClosure, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, Arguments.s_launchArgumentType);
        }
    }

    public static Coroutine<Coroutine> Launch(Func<Coroutine> provider) =>
        LaunchInternal(provider, providerClosure: null);

    public static Coroutine<Coroutine<TResult>> Launch<TResult>(Func<Coroutine<TResult>> provider) =>
        LaunchInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal readonly struct LaunchArgument(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine> completionSource) : ICallbackArgument
        {
            private readonly ValueTaskCompletionSource<Coroutine> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallbackArgument.CompletionSource => _completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ValueTaskCompletionSource<object?>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();
                coroutineContext.TreatAsNewSibling();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref coroutineContext);
                coroutineContext.ResultStateMachine.AwaitUnsafeOnCompletedThenContinueWith(ref coroutineAwaiter, () => {
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

        internal readonly struct LaunchArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine<TResult>> completionSource) : ICallbackArgument
        {
            private readonly ValueTaskCompletionSource<Coroutine<TResult>> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallbackArgument.CompletionSource => _completionSource;

            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateGenericValueTask();
                coroutineContext.TreatAsNewSibling();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref coroutineContext);
                coroutineContext.ResultStateMachine.AwaitUnsafeOnCompletedThenContinueWith(ref coroutineAwaiter, () => {
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
