using System.Reflection;
using Vernuntii.Coroutines.Reflection;

namespace Vernuntii.Coroutines;

internal delegate ref CoroutineMethodBuilder<TResult> GetStateMachineMethodBuilderByRefDelegate<TStateMachine, TResult>(ref TStateMachine stateMachine);
internal delegate CoroutineMethodBuilder<TResult> GetStateMachineMethodBuilderDelegate<TStateMachine, TResult>(in TStateMachine stateMachine);

internal static class CoroutineStateMachineAccessor<[DAM(StateMachineMemberTypes)] TStateMachine, TResult>
    where TStateMachine : IAsyncStateMachine
{
    private static readonly Type s_methodBuilderType = typeof(CoroutineMethodBuilder<TResult>);
    private static readonly FieldInfo? s_methodBuilderFieldInfo;
    private static readonly CloneCoroutineStateMachineDelegate<TStateMachine> s_cloneStateMachineDelegate;
    private static readonly FieldAccessor<TStateMachine, CoroutineMethodBuilder<TResult>>? s_coroutineMethodBuilderAccessor;

    static CoroutineStateMachineAccessor()
    {
        if (GlobalRuntimeFeature.IsDynamicCodeSupported) {
            s_cloneStateMachineDelegate = CoroutineStateMachineAccessorCore<TStateMachine>.CompileCloneStateMachineDelegate(ref s_methodBuilderFieldInfo, s_methodBuilderType);
        } else {
            s_cloneStateMachineDelegate = CoroutineStateMachineAccessorCore<TStateMachine>.CloneStateMachineInCompiledRuntime;
            s_methodBuilderFieldInfo = CoroutineStateMachineAccessorCore<TStateMachine>.GetFirstFieldByType(s_methodBuilderType);
        }

        if (s_methodBuilderFieldInfo is not null) {
            s_coroutineMethodBuilderAccessor = new FieldAccessor<TStateMachine, CoroutineMethodBuilder<TResult>>(
                CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType,
                s_methodBuilderType,
                s_methodBuilderFieldInfo);
        }
    }

    public static CloneCoroutineStateMachineDelegate<TStateMachine> CloneStateMachine {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_cloneStateMachineDelegate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if state machine does not have method builder.</exception>
    public static FieldAccessor<TStateMachine, CoroutineMethodBuilder<TResult>> CoroutineMethodBuilderAccessor {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_coroutineMethodBuilderAccessor ?? throw FieldAccessor<TStateMachine, CoroutineMethodBuilder<TResult>>.Exceptions.FieldArityIsNotOne();
    }
}
