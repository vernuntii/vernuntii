namespace Vernuntii.PluginSystem.Reactive;

public interface IDistinguishableEventFulfiller
{
    Task FullfillAsync<T>(object eventId, T eventData);
}
