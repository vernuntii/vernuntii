using System.Reflection;

namespace Vernuntii.Coroutines;

internal static class CoroutineStateMachineAccessorCore<TStateMachine> where TStateMachine : IAsyncStateMachine
{
    internal static readonly Type s_stateMachineType = typeof(TStateMachine);
    internal static readonly FieldInfo[] s_stateMachineFieldInfos;

    static CoroutineStateMachineAccessorCore() => s_stateMachineFieldInfos = s_stateMachineType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
}
