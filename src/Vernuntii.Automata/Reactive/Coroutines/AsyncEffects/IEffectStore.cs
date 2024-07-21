namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public interface IEffectStore
{
    IReadOnlyCollection<EffectHandlerId> CompiledEffects { get; }

    ValueTask HandleAsync(IEffect step);
}
