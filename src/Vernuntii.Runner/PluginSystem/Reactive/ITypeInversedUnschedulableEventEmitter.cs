namespace Vernuntii.PluginSystem.Reactive;

internal interface ITypeInversedUnschedulableEventEmitter
{
    void Emit<T>(EventEmissionContext context, T eventData);
}
