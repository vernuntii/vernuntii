namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

//public interface IStep<T> : 
//{
//    StepHandlerId HandlerId { get; }

//    Task<EventTrace<T>> GetAwaiter();
//}

public interface IEffect {
    EffectHandlerId HandlerId { get; }
}
