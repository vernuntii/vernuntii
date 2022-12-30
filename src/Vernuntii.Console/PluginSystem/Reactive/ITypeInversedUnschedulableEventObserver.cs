namespace Vernuntii.PluginSystem.Reactive;

internal interface ITypeInversedUnschedulableEventObserver
{
    void OnFulfillment<TActual>(EventFulfillmentContext context, TActual actualEventData);
}
