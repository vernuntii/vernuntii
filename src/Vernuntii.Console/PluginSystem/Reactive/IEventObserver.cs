namespace Vernuntii.PluginSystem.Reactive;

internal interface IEventObserver
{
    void OnFulfillment<TActual>(EventFulfillmentContext context, TActual actualEventData);
}
