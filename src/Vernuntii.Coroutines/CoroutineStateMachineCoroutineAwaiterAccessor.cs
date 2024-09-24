using System.Reflection;
using Vernuntii.Coroutines.Reflection;

namespace Vernuntii.Coroutines;

internal static class CoroutineStateMachineCoroutineAwaiterAccessor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] TStateMachine, TCoroutineAwaiter>
    where TStateMachine : IAsyncStateMachine
{
    private static readonly Type s_coroutineAwaiterType;
    private static readonly FieldInfo? s_coroutineAwaiterFieldInfo;
    private static readonly FieldAccessor<TStateMachine, TCoroutineAwaiter>? s_coroutineAwaiterAccessor;

    static CoroutineStateMachineCoroutineAwaiterAccessor()
    {
        s_coroutineAwaiterType = typeof(TCoroutineAwaiter);
        s_coroutineAwaiterFieldInfo = CoroutineStateMachineAccessorCore<TStateMachine>.GetFirstFieldByType(s_coroutineAwaiterType);
        if (s_coroutineAwaiterFieldInfo is not null) {
            s_coroutineAwaiterAccessor = new FieldAccessor<TStateMachine, TCoroutineAwaiter>(
                CoroutineStateMachineAccessorCore<TStateMachine>.s_stateMachineType,
                s_coroutineAwaiterType,
                s_coroutineAwaiterFieldInfo);
        }
    }

    internal static FieldAccessor<TStateMachine, TCoroutineAwaiter> CoroutineAwaiterAccessor {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => s_coroutineAwaiterAccessor ?? throw FieldAccessor<TStateMachine, TCoroutineAwaiter>.Exceptions.FieldArityIsNotOne();
    }
}
