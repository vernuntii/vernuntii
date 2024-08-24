using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> SpawnInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<Coroutine>.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.s_spawnArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> SpawnInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<Coroutine<TResult>>.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument<TResult>(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.s_spawnArgumentType);
        }
    }

    public static Coroutine<Coroutine> Spawn(Func<Coroutine> provider) =>
        SpawnInternal(provider, providerClosure: null);

    public static Coroutine<Coroutine<TResult>> Spawn<TResult>(Func<Coroutine<TResult>> provider) =>
        SpawnInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal readonly struct SpawnArgument(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine> completionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine>(provider);
                }
                Coroutine coroutineAsReplacement;

                var childContext = coroutineContext;
                childContext._bequeathBehaviour = CoroutineContextBequeathBehaviour.Undefined;

                if (!coroutine.IsChildCoroutine) {
                    coroutineAsReplacement = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref childContext);
                } else {
                    coroutineAsReplacement = coroutine;
                }
                var coroutineAsReplacementAwaiter = coroutineAsReplacement.ConfigureAwait(false).GetAwaiter();

                var intermediateCompletionSource = ValueTaskCompletionSource<object?>.RentFromCache();
                coroutineAsReplacement._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAsReplacementAwaiter, ref childContext);
                coroutineAsReplacementAwaiter.UnsafeOnCompleted(() => {
                    try {
                        coroutineAsReplacementAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                coroutineAsReplacement.MarkCoroutineAsHandled();
                completionSource.SetResult(coroutineAsReplacement);
            }
        }

        internal readonly struct SpawnArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine<TResult>> completionSource) : ICallbackArgument
        {
            void ICallbackArgument.Callback(ref CoroutineContext coroutineContext)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(provider);
                }
                Coroutine<TResult> childCoroutine;

                var childContext = coroutineContext;
                childContext._bequeathBehaviour = CoroutineContextBequeathBehaviour.Undefined;

                if (!coroutine.IsChildCoroutine) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref childContext);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();

                var intermediateCompletionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
                childCoroutine._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref childCoroutineAwaiter, ref childContext);
                childCoroutineAwaiter.UnsafeOnCompleted(() => {
                    try {
                        var result = childCoroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                childCoroutine.MarkCoroutineAsHandled();
                completionSource.SetResult(childCoroutine);
            }
        }
    }
}
