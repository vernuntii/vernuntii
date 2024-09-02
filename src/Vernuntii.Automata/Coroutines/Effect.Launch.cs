using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.Iterators;

namespace Vernuntii.Coroutines;

partial class Effect
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine> LaunchInternal(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<Coroutine>.RentFromCache();
        return new Coroutine<Coroutine>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(in Arguments.s_launchArgumentType, in argument, completionSource);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Coroutine<Coroutine<TResult>> LaunchInternal<TResult>(Delegate provider, IClosure? providerClosure)
    {
        var completionSource = ValueTaskCompletionSource<Coroutine<TResult>>.RentFromCache();
        return new Coroutine<Coroutine<TResult>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.LaunchArgument<TResult>(provider, providerClosure, completionSource);
            argumentReceiver.ReceiveCallableArgument(Arguments.s_launchArgumentType, in argument, completionSource);
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
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ValueTaskCompletionSource<object?>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateValueTask();

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                contextToBequest.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
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
                var coroutineAwaiter = coroutine.ConfigureAwait(false).GetAwaiter();
                var intermediateCompletionSource = ValueTaskCompletionSource<TResult>.RentFromCache();
                coroutine._task = intermediateCompletionSource.CreateGenericValueTask();

                CoroutineContext contextToBequest = default;
                contextToBequest.TreatAsNewSibling();
                CoroutineContext.InheritOrBequestCoroutineContext(ref contextToBequest, in context);

                CoroutineMethodBuilderCore.PreprocessCoroutine(ref coroutineAwaiter, ref contextToBequest);
                contextToBequest.ResultStateMachine.CallbackWhenForkCompletedUnsafely(ref coroutineAwaiter, () => {
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
