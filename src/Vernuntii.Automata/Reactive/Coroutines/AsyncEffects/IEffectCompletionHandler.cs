namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal interface IEffectCompletionHandler
{
    IEffect Effect { get; }

    void CompleteEffect();
}
