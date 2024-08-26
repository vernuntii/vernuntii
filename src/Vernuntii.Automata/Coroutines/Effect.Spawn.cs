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
            argumentReceiver.ReceiveCallableArgument(in argument, in Arguments.s_spawnArgumentType);
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
            argumentReceiver.ReceiveCallableArgument(in argument, in Arguments.s_spawnArgumentType);
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
            ValueTaskCompletionSource<Coroutine> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<Coroutine> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallableArgument.CompletionSource => _completionSource;

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine>(provider);
                }

                var contextToBequest = context;
                contextToBequest.TreatAsNewChild();

                Coroutine coroutineAsReplacement;
                if (!coroutine.IsChildCoroutine) {
                    coroutineAsReplacement = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref contextToBequest);
                } else {
                    coroutineAsReplacement = coroutine;
                }
                var coroutineAsReplacementAwaiter = coroutineAsReplacement.ConfigureAwait(false).GetAwaiter();
                CoroutineContext.InheritirBequestCoroutineContext(ref contextToBequest, in context);

                var intermediateCompletionSource = ValueTaskCompletionSource<object?>.RentFromCache();
                coroutineAsReplacement._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAsReplacementAwaiter, ref contextToBequest);
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
                _completionSource.SetResult(coroutineAsReplacement);
            }
        }

        internal readonly struct SpawnArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine<TResult>> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<Coroutine<TResult>> _completionSource = completionSource;

            readonly ICoroutineCompletionSource ICallableArgument.CompletionSource => _completionSource;

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(provider);
                }

                var contextToBequest = context;
                contextToBequest.TreatAsNewChild();

                Coroutine<TResult> childCoroutine;
                if (!coroutine.IsChildCoroutine) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref contextToBequest);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();
                CoroutineContext.InheritirBequestCoroutineContext(ref contextToBequest, in context);

                var intermediateCompletionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
                childCoroutine._task = intermediateCompletionSource.CreateGenericValueTask();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref childCoroutineAwaiter, ref contextToBequest);
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
                _completionSource.SetResult(childCoroutine);
            }
        }
    }
}
