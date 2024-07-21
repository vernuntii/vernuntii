namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public interface  IEffectHandler
{
    ValueTask HandleAsync(IEffect step);
}
