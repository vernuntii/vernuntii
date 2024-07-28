namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public interface IAllEffectResult
{
    //AllEffectResultProperty[] FirstLevelProperties { get; }
}

public class AllEffectResultProperty(string propertyName, bool isAwaitable)
{
    string PropertyName { get; } = propertyName;
    bool IsAwaitable { get; } = isAwaitable;
}
