using System.Reflection;
using System.Reflection.Emit;

namespace Vernuntii.Coroutines;

internal delegate TStateMachine CloneCoroutineStateMachineDelegate<TStateMachine>(in TStateMachine stateMachine);
internal delegate ref CoroutineMethodBuilder<TResult> GetStateMachineMethodBuilderDelegate<TStateMachine, TResult>(ref TStateMachine stateMachine);

internal static class CoroutineStateMachineAccessor<TStateMachine, TResult> where TStateMachine : IAsyncStateMachine
{
    private static readonly Type s_methodBuilderType = typeof(CoroutineMethodBuilder<TResult>);
    private static readonly FieldInfo? s_methodBuilderFieldInfo;
    private static readonly CloneCoroutineStateMachineDelegate<TStateMachine> s_cloneStateMachineDelegate;
    private static readonly GetStateMachineMethodBuilderDelegate<TStateMachine, TResult>? s_getCoroutineMethodBuilder;

    static CoroutineStateMachineAccessor()
    {
        s_cloneStateMachineDelegate = CompileCloneStateMachineDelegate(ref s_methodBuilderFieldInfo);
        if (s_methodBuilderFieldInfo is not null) {
            s_getCoroutineMethodBuilder = CompileGetMethodBuilderDelegate(s_methodBuilderFieldInfo);
        }
    }

    private static CloneCoroutineStateMachineDelegate<TStateMachine> CompileCloneStateMachineDelegate(ref FieldInfo? builderFieldInfo)
    {
        var stateMachineType = CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType;
        var method = new DynamicMethod(
            nameof(CloneStateMachine),
            returnType: stateMachineType,
            parameterTypes: [stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        var loc1 = il.DeclareLocal(stateMachineType);
        if (stateMachineType.IsValueType) {
            il.Emit(OpCodes.Ldloca_S, loc1);
            il.Emit(OpCodes.Initobj, stateMachineType);
        } else {
            var defaultConstructor = stateMachineType.GetConstructor([]);
            il.Emit(OpCodes.Newobj, defaultConstructor!);
            il.Emit(OpCodes.Stloc, loc1);
        }
        foreach (var field in CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineFieldInfos) {
            if (builderFieldInfo is null && field.FieldType == s_methodBuilderType) {
                builderFieldInfo = field;
            }
            if (stateMachineType.IsValueType) {
                il.Emit(OpCodes.Ldloca_S, loc1);
            } else {
                il.Emit(OpCodes.Ldloc, loc1);
            }
            il.Emit(OpCodes.Ldarg_0);
            if (!stateMachineType.IsValueType) {
                il.Emit(OpCodes.Ldind_Ref);
            }
            // Only fields are relevant
            il.Emit(OpCodes.Ldfld, field);
            il.Emit(OpCodes.Stfld, field);
        }
        il.Emit(OpCodes.Ldloc, loc1);
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<CloneCoroutineStateMachineDelegate<TStateMachine>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TStateMachine CloneStateMachine(in TStateMachine stateMachine) => s_cloneStateMachineDelegate(in stateMachine);

    private static GetStateMachineMethodBuilderDelegate<TStateMachine, TResult> CompileGetMethodBuilderDelegate(FieldInfo builderFieldInfo)
    {
        var stateMachineType = CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType;
        var method = new DynamicMethod(
            nameof(GetCoroutineMethodBuilder),
            returnType: s_methodBuilderType.MakeByRefType(),
            parameterTypes: [stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load argument (stateMachine) onto the stack
        if (!stateMachineType.IsValueType) {
            il.Emit(OpCodes.Ldind_Ref); // Indicate that the argument is a reference type
        }
        il.Emit(OpCodes.Ldflda, builderFieldInfo); // Load address of the field into the stack
        il.Emit(OpCodes.Ret); // Return the address of the field(by reference)
        return method.CreateDelegate<GetStateMachineMethodBuilderDelegate<TStateMachine, TResult>>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref CoroutineMethodBuilder<TResult> GetCoroutineMethodBuilder(ref TStateMachine stateMachine)
    {
        if (s_getCoroutineMethodBuilder is null) {
            throw new InvalidOperationException();
        }

        return ref s_getCoroutineMethodBuilder.Invoke(ref stateMachine);
    }
}
