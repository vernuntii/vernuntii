using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.Iterators;

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
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_spawnArgumentType, in argument, completionSource);
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
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_spawnArgumentType, in argument, completionSource);
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

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine>(provider);
                }

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewChild();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                Coroutine childCoroutine;
                if (!coroutine.IsChildCoroutine) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref contextToBequest);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();

                var intermediateCompletionSource = ValueTaskCompletionSource<object?>.RentFromCache();
                childCoroutine._task = intermediateCompletionSource.CreateValueTask();
                CoroutineMethodBuilderCore.PreprocessCoroutine(ref childCoroutineAwaiter, ref contextToBequest);
                childCoroutineAwaiter.UnsafeOnCompleted(() => {
                    try {
                        childCoroutineAwaiter.GetResult();
                        intermediateCompletionSource.SetResult(default);
                    } catch (Exception error) {
                        intermediateCompletionSource.SetException(error);
                        throw; // Must bubble up
                    }
                });
                childCoroutine.MarkCoroutineAsHandled();
                _completionSource.SetResult(childCoroutine);
            }
        }

        internal readonly struct SpawnArgument<TResult>(
            Delegate provider,
            IClosure? providerClosure,
            ValueTaskCompletionSource<Coroutine<TResult>> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<Coroutine<TResult>> _completionSource = completionSource;

            void ICallableArgument.Callback(in CoroutineContext context)
            {
                Coroutine<TResult> coroutine;
                if (providerClosure is null) {
                    var typedProvider = Unsafe.As<Func<Coroutine<TResult>>>(provider);
                    coroutine = typedProvider();
                } else {
                    coroutine = providerClosure.InvokeDelegateWithClosure<Coroutine<TResult>>(provider);
                }

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewChild();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                Coroutine<TResult> childCoroutine;
                if (!coroutine.IsChildCoroutine) {
                    childCoroutine = CoroutineMethodBuilderCore.MakeChildCoroutine(ref coroutine, ref contextToBequest);
                } else {
                    childCoroutine = coroutine;
                }
                var childCoroutineAwaiter = childCoroutine.ConfigureAwait(false).GetAwaiter();

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
