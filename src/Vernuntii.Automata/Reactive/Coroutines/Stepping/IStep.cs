namespace Vernuntii.Reactive.Coroutines.Stepping;

//public interface IStep<T> : 
//{
//    StepHandlerId HandlerId { get; }

//    Task<EventTrace<T>> GetAwaiter();
//}

public interface IStep {
    StepHandlerId HandlerId { get; }
}
