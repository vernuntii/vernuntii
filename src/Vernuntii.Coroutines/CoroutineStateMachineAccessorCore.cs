using System.Reflection;
using System.Reflection.Emit;

namespace Vernuntii.Coroutines;

internal static class CoroutineStateMachineAccessorCore<[DAM(StateMachineMemberTypes)] TStateMachine> 
    where TStateMachine : IAsyncStateMachine
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    internal static readonly Type s_stateMachineType = typeof(TStateMachine);
    internal static readonly FieldInfo[] s_stateMachineFieldInfos;

    static CoroutineStateMachineAccessorCore() => s_stateMachineFieldInfos = s_stateMachineType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    internal static CloneCoroutineStateMachineDelegate<TStateMachine> CompileCloneStateMachineDelegate(ref FieldInfo? methodBuilderFieldInfo, Type methodBuilderType)
    {
        var stateMachineType = s_stateMachineType;
        var method = new DynamicMethod(
            "CloneStateMachine",
            returnType: stateMachineType,
            parameterTypes: [stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        var loc1 = il.DeclareLocal(stateMachineType);
        if (stateMachineType.IsValueType) {
            il.Emit(OpCodes.Ldloca_S, loc1);
            il.Emit(OpCodes.Initobj, stateMachineType);
        } else {
            var defaultConstructor = stateMachineType.GetConstructor(Type.EmptyTypes);
            il.Emit(OpCodes.Newobj, defaultConstructor!);
            il.Emit(OpCodes.Stloc, loc1);
        }
        foreach (var field in s_stateMachineFieldInfos) {
            if (methodBuilderFieldInfo is null && field.FieldType == methodBuilderType) {
                methodBuilderFieldInfo = field;
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

    internal static TStateMachine CloneStateMachineInCompiledRuntime(in TStateMachine stateMachine)
    {
        var stateMachineRef = __makeref(Unsafe.AsRef(in stateMachine));
        var stateMachineReplica = Activator.CreateInstance<TStateMachine>();
        var stateMachineReplicaRef = __makeref(stateMachineReplica);

        foreach (var field in s_stateMachineFieldInfos) {
            field.SetValueDirect(stateMachineReplicaRef, field.GetValueDirect(stateMachineRef)!);
        }

        return stateMachineReplica;
    }

    internal static FieldInfo? GetFirstFieldByType(Type methodBuilderType)
    {
        foreach (var field in s_stateMachineFieldInfos) {
            if (field.FieldType == methodBuilderType) {
                return field;
            }
        }

        return null;
    }

    internal static TDelegate CompileGetMethodBuilderDelegate<TDelegate>(FieldInfo builderFieldInfo, Type methodBuilderType) where TDelegate : Delegate
    {
        var stateMachineType = s_stateMachineType;
        var method = new DynamicMethod(
            "GetCoroutineMethodBuilder",
            returnType: methodBuilderType.MakeByRefType(),
            parameterTypes: [stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load argument (stateMachine) onto the stack
        if (!stateMachineType.IsValueType) {
            il.Emit(OpCodes.Ldind_Ref); // Indicate that the argument is a reference type
        }
        il.Emit(OpCodes.Ldflda, builderFieldInfo); // Load address of the field into the stack
        il.Emit(OpCodes.Ret); // Return the address of the field (by reference)
        return method.CreateDelegate<TDelegate>();
    }
}
