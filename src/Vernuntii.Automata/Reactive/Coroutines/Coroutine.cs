using System.Runtime.CompilerServices;

namespace Vernuntii.Reactive.Coroutines;

[AsyncMethodBuilder(typeof(CoroutineMethodBuilder))]
public class Coroutine(Task Task, CoroutineMethodBuilder.CoroutineSite site)
{
    public CoroutineMethodBuilder.CoroutineSite Site { get; } = site;

    public CoroutineAwaiter GetAwaiter() => new CoroutineAwaiter(Task.GetAwaiter(), Site);

    public struct CoroutineAwaiter(TaskAwaiter awaiter, CoroutineMethodBuilder.CoroutineSite site) : ICriticalNotifyCompletion
    {
        public readonly bool IsCompleted => awaiter.IsCompleted;

        public CoroutineMethodBuilder.CoroutineSite Site { get; } = site;

        public void GetResult() => awaiter.GetResult();

        public void OnCompleted(Action continuation) => awaiter.OnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation) => awaiter.UnsafeOnCompleted(continuation);
    }
}
