namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal interface IEventTrace
{
    int Id { get; set; }

    bool HasId { get; }
}
