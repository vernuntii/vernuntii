namespace Vernuntii.PluginSystem.Reactive;

internal sealed class TypeInversedUnschedulableEventEmitter<TExpected> : ITypeInversedUnschedulableEventEmitter
{
    private readonly IEventEmitter<TExpected> _eventHandler;

    public TypeInversedUnschedulableEventEmitter(IEventEmitter<TExpected> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public void Emit<TActual>(EventEmissionContext context, TActual actualEventData)
    {
        if (actualEventData is null) {
            _eventHandler.Emit(context, default!);
            return;
        }

        if (actualEventData is not TExpected expectedEventData) {
            throw new InvalidOperationException(@$"The type of the actual event data does not match with the expected type
Actual type = {typeof(TExpected)}
Expected type = {typeof(TActual)}
");
        }

        _eventHandler.Emit(context, expectedEventData);
    }
}
