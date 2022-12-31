namespace Vernuntii.PluginSystem.Reactive;

public interface IUniqueEventFulfiller
{
    Task FullfillAsync<T>(object eventId, T eventData);
}
