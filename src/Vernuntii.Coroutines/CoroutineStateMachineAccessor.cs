using System.Reflection;
using Vernuntii.Coroutines.Reflection;

namespace Vernuntii.Coroutines;

internal delegate TStateMachine CloneCoroutineStateMachineDelegate<TStateMachine>(in TStateMachine stateMachine);
internal delegate ref CoroutineMethodBuilder GetStateMachineMethodBuilderByRefDelegate<TStateMachine>(ref TStateMachine stateMachine);
internal delegate CoroutineMethodBuilder GetStateMachineMethodBuilderDelegate<TStateMachine>(in TStateMachine stateMachine);

internal static class CoroutineStateMachineAccessor<[DAM(StateMachineMemberTypes)] TStateMachine>
    where TStateMachine : IAsyncStateMachine
{
    private static readonly Type s_methodBuilderType = typeof(CoroutineMethodBuilder);
    private static readonly FieldInfo? s_methodBuilderFieldInfo;
    private static readonly CloneCoroutineStateMachineDelegate<TStateMachine> s_cloneStateMachineDelegate;
    private static readonly FieldAccessor<TStateMachine, CoroutineMethodBuilder>? s_coroutineMethodBuilderAccessor;

    static CoroutineStateMachineAccessor()
    {
        if (GlobalRuntimeFeature.IsDynamicCodeSupported) {
            s_cloneStateMachineDelegate = CoroutineStateMachineAccessorCore<TStateMachine>.CompileCloneStateMachineDelegate(ref s_methodBuilderFieldInfo, s_methodBuilderType);
        } else {
            s_cloneStateMachineDelegate = CoroutineStateMachineAccessorCore<TStateMachine>.CloneStateMachineInCompiledRuntime;
            s_methodBuilderFieldInfo = CoroutineStateMachineAccessorCore<TStateMachine>.GetFirstFieldByType(s_methodBuilderType);
        }

        if (s_methodBuilderFieldInfo is not null) {
            s_coroutineMethodBuilderAccessor = new FieldAccessor<TStateMachine, CoroutineMethodBuilder>(
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
    public static FieldAccessor<TStateMachine, CoroutineMethodBuilder> CoroutineMethodBuilderAccessor {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_coroutineMethodBuilderAccessor ?? throw FieldAccessor<TStateMachine, CoroutineMethodBuilder>.Exceptions.FieldArityIsNotOne();
    }
}
