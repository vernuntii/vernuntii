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
        s_coroutineAwaiterFieldInfo = CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineFieldInfos.FirstOrDefault();
        s_getCoroutineAwaiter = CompileGetStateMachineCoroutineAwaiterDelegate();
    }

    internal static GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter> GetCoroutineAwaiter {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_getCoroutineAwaiter ?? throw new NotImplementedException($"The state machine of type {typeof(TStateMachine)} does not implement a field of type {typeof(TCoroutineAwaiter)}");
    }

    private static GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter>? CompileGetStateMachineCoroutineAwaiterDelegate()
    {
        if (s_coroutineAwaiterFieldInfo is null) {
            return null;
        }
        var method = new DynamicMethod(
            nameof(GetCoroutineAwaiter),
            null,
            parameterTypes: [CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType.MakeByRefType()],
            restrictedSkipVisibility: true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0); // Load argument (stateMachine) onto the stack
        il.Emit(OpCodes.Ldflda, s_coroutineAwaiterFieldInfo); // Load address of the field into the stack
        il.Emit(OpCodes.Ret); // Return the address of the field(by reference)
        return method.CreateDelegate<GetStateMachineCoroutineAwaiterDelegate<TStateMachine, TCoroutineAwaiter>>();
    }
}
