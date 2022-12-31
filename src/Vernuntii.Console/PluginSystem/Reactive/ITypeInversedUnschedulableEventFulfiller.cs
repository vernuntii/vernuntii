namespace Vernuntii.PluginSystem.Reactive;

internal interface ITypeInversedUnschedulableEventFulfiller
{
    void Fulfill<T>(EventFulfillmentContext context, T eventData);
}
