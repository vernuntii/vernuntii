namespace Vernuntii.Reactive.Events;

internal sealed record EventEmissionBacklog
{
    public IReadOnlyCollection<(Func<object, Task>, object)> Collection =>
        _collection;

    private readonly List<(Func<object, Task>, object)> _collection = new();

    public void AddEventEmission(Func<object, Task> eventObserver, object eventData) =>
        _collection.Add((eventObserver, eventData));
}
