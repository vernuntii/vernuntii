namespace Vernuntii.Reactive.Coroutines.Stepping;

public interface IStepAwaiterProvider<T>
{
    T GetAwaiter();
}
