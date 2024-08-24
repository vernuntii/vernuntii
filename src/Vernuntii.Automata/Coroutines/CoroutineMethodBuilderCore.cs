using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineMethodBuilderCore
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void PreprocessCoroutine<TCoroutine, TCoroutinePreprocessor>(
        ref TCoroutine coroutine,
        ref TCoroutinePreprocessor preprocessor)
        where TCoroutine : IRelativeCoroutine
        where TCoroutinePreprocessor : ICoroutinePreprocessor
    {
        if (coroutine.IsChildCoroutine) {
            preprocessor.PreprocessChildCoroutine(ref coroutine);
        } else if (coroutine.IsSiblingCoroutine) {
            preprocessor.PreprocessSiblingCoroutine(ref coroutine);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static void PreprocessAwaiterIfCoroutine<TAwaiter, Preprocessor>(
        ref TAwaiter awaiter,
        ref Preprocessor preprocessor)
        where Preprocessor : ICoroutinePreprocessor
    {
        if (null != default(TAwaiter) && awaiter is ICoroutineAwaiter) {
            ref var coroutineAwaiter = ref Unsafe.As<TAwaiter, Coroutine.CoroutineAwaiter>(ref awaiter);
            PreprocessCoroutine(ref coroutineAwaiter, ref preprocessor);
        }
    }

    internal static Coroutine MakeChildCoroutine(ref Coroutine nonChildCoroutine, ref CoroutineContext childContext)
    {
        var coroutineAwaiter = nonChildCoroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<VoidResult>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder>(coroutineBuilder);
        stateMachine.State = -1;
        stateMachineBox.StateMachine = stateMachine;
        stateMachineBox.MoveNext();
        childContext.SetResultStateMachine(stateMachineBox);
        return new Coroutine(new ValueTask(stateMachineBox, stateMachineBox.Version));
    }

    internal static Coroutine<T> MakeChildCoroutine<T>(ref Coroutine<T> nonChildCoroutine, ref CoroutineContext childContext)
    {
        var coroutineAwaiter = nonChildCoroutine.ConfigureAwait(false).GetAwaiter();
        var stateMachineBox = CoroutineMethodBuilder<T>.CoroutineStateMachineBox<CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>>.RentFromCache();
        var coroutineBuilder = new CoroutineAwaiterMethodBuilder<T>(in coroutineAwaiter, stateMachineBox);
        var stateMachine = new CoroutineAwaiterStateMachine<CoroutineAwaiterMethodBuilder<T>>(coroutineBuilder);
        stateMachine.State = -1;
        stateMachineBox.StateMachine = stateMachine;
        stateMachineBox.MoveNext();
        childContext.SetResultStateMachine(stateMachineBox);
        return new Coroutine<T>(new ValueTask<T>(stateMachineBox, stateMachineBox.Version));
    }
}
