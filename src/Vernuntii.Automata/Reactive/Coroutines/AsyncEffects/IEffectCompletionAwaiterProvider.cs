namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public interface IEffectCompletionAwaiterProvider<T>
{
    T GetAwaiter();
}
