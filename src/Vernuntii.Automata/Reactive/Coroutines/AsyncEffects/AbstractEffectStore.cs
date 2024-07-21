namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal abstract class AbstractEffectStore : IEffectStore
{
    public IReadOnlyCollection<EffectHandlerId> CompiledEffects => _compiledEffects ??= EffectHandlers.Keys;

    protected abstract Dictionary<EffectHandlerId, IEffectHandler> EffectHandlers { get; }

    private IReadOnlyCollection<EffectHandlerId>? _compiledEffects;

    public ValueTask HandleAsync(IEffect effect)
    {
        if (!EffectHandlers.TryGetValue(effect.HandlerId, out var effectHandler)) {
            throw new InvalidOperationException($"Step {effect.HandlerId} is not stored");
        }

        return effectHandler.HandleAsync(effect);
    }
}
