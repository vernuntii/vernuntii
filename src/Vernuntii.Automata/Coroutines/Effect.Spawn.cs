using System.Runtime.CompilerServices;
using System.Text;
using Vernuntii.Coroutines.v1;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> SpawnInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = Coroutine<Coroutine>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.SpawnArgumentType);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> SpawnInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = Coroutine<Coroutine<TResult>>.CompletionSource.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.SpawnArgument<TResult>(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallbackArgument(in argument, in Arguments.SpawnArgumentType);
        }
    }

    public static Coroutine<Coroutine> Spawn(Func<Coroutine> provider) =>
        SpawnInternal(provider, providerClosure: null);

    public static Coroutine<Coroutine<TResult>> Spawn<TResult>(Func<Coroutine<TResult>> provider) =>
        SpawnInternal<TResult>(provider, providerClosure: null);

    partial class Arguments
    {
        internal readonly static Key SpawnArgumentType = new(Encoding.ASCII.GetBytes("@vernuntii"), Encoding.ASCII.GetBytes("spawn"));

        internal readonly struct SpawnArgument(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<Coroutine>.CompletionSource completionSource) : ICallbackArgument
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
                Coroutine coroutineAsSupplement;

                var coroutineNodeAsCompletary = new CoroutineContext(coroutineContext._scope);
                if (!coroutine.IsChildCoroutine) {
                    coroutineAsSupplement = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                } else {
                    coroutineAsSupplement = coroutine;
                }
                var coroutineAsComplementaryAwaiter = coroutineAsSupplement.GetAwaiter();

                var intermediateCompletionSource = Coroutine<object?>.CompletionSource.RentFromCache();
                coroutineAsSupplement._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                coroutineAsComplementaryAwaiter.UnsafeOnCompleted(() => {
                    try {
                        coroutineAsComplementaryAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                completionSource.SetResult(coroutineAsSupplement);
            }
        }

        internal readonly struct SpawnArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            Coroutine<Coroutine<TResult>>.CompletionSource completionSource) : ICallbackArgument
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
                Coroutine<TResult> coroutineAsSupplement;

                var coroutineNodeAsCompletary = new CoroutineContext(coroutineContext._scope);
                if (!coroutine.IsChildCoroutine) {
                    coroutineAsSupplement = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                } else {
                    coroutineAsSupplement = coroutine;
                }
                var coroutineAsComplementaryAwaiter = coroutineAsSupplement.GetAwaiter();

                var intermediateCompletionSource = Coroutine<TResult>.CompletionSource.RentFromCache();
                coroutineAsSupplement._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.HandleCoroutine(ref coroutine, ref coroutineNodeAsCompletary);
                coroutineAsComplementaryAwaiter.UnsafeOnCompleted(() => {
                    try {
                        var result = coroutineAsComplementaryAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(result);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                completionSource.SetResult(coroutineAsSupplement);
            }
        }
    }
}
