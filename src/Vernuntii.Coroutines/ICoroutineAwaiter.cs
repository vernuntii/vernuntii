namespace Vernuntii.Coroutines;

internal interface ICoroutineAwaiter
{
    bool IsCompleted { get; }
    void GetResult();
}
