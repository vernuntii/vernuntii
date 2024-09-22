using System.Reflection;
using System.Reflection.Emit;

namespace Vernuntii.Coroutines;

delegate ref TCoroutineAwaiter GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine;

internal static class CoroutineStateMachineCoroutineAwaiterAccessor<TStateMachine, TCoroutineAwaiter> where TStateMachine : IAsyncStateMachine
{
    private static readonly Type s_coroutineAwaiterType;
    private static readonly FieldInfo? s_coroutineAwaiterFieldInfo;
    private static readonly GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter>? s_getCoroutineAwaiter;

    static CoroutineStateMachineCoroutineAwaiterAccessor()
    {
        s_coroutineAwaiterType = typeof(TCoroutineAwaiter);
        s_coroutineAwaiterFieldInfo = CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineFieldInfos.SingleOrDefault(fieldInfo => fieldInfo.FieldType == s_coroutineAwaiterType);
        if (s_coroutineAwaiterFieldInfo is not null) {
            s_getCoroutineAwaiter = CompileGetStateMachineCoroutineAwaiterDelegate(s_coroutineAwaiterFieldInfo);
        }
    }

    internal static GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter> GetCoroutineAwaiter {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_getCoroutineAwaiter ?? throw new NotImplementedException($"The state machine of type {typeof(TStateMachine)} either does not implement a field of type {typeof(TCoroutineAwaiter)} or implements more than one field of this type");
    }

    private static GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter> CompileGetStateMachineCoroutineAwaiterDelegate(FieldInfo coroutineAwaiterFieldInfo)
    {
        var stateMachineType = CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType;
        var method = new DynamicMethod(
            nameof(GetCoroutineAwaiter),
            returnType: s_coroutineAwaiterType.MakeByRefType(),
            parameterTypes: [stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load argument (stateMachine) onto the stack
        if (!stateMachineType.IsValueType) {
            il.Emit(OpCodes.Ldind_Ref); // Indicate that the argument is a reference type
        }
        il.Emit(OpCodes.Ldflda, coroutineAwaiterFieldInfo); // Load address of the field into the stack
        il.Emit(OpCodes.Ret); // Return the address of the field(by reference)
        return method.CreateDelegate<GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter>>();
    }
}
