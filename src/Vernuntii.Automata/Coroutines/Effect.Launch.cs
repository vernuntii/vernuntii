using System.Runtime.CompilerServices;
using System.Text;
using Vernuntii.Coroutines.v1;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> LaunchInternal(Delegate provider, IClosure? providerClosure)
    {
        var immediateCompletionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument(provider, providerClosure, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.LaunchArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> LaunchInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var immediateCompletionSource = Coroutine<Coroutine<TResult>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(immediateCompletionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<TResult>(provider, providerClosure, immediateCompletionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, Arguments.LaunchArgumentType);
        }
    }

    public static Coroutine<Coroutine> Launch(Func<Coroutine> provider) =>
        LaunchInternal(provider, providerClosure: null);

    public static Coroutine<Coroutine<TResult>> Launch<TResult>(Func<Coroutine<TResult>> provider) =>
        LaunchInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal readonly static Key LaunchArgumentType = new(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("launch"));

        internal readonly struct LaunchArgument(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<Coroutine>.CompletionSource immediateCompletionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeClosured<Coroutine>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = Coroutine<object?>.CompletionSource.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineContext);
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
                immediateCompletionSource.SetResult(coroutine);
            }
        }

        internal readonly struct LaunchArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<Coroutine<TResult>>.CompletionSource immediateCompletionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeClosured<Coroutine<TResult>>(provider);
                }
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = Coroutine<TResult>.CompletionSource.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutineAwaiter, ref coroutineContext);
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
                immediateCompletionSource.SetResult(coroutine);
            }
        }
    }
}
