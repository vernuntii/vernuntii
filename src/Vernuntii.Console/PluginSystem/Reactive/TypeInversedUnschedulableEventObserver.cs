namespace Vernuntii.PluginSystem.Reactive;

internal sealed class TypeInversedUnschedulableEventObserver<TExpected> : ITypeInversedUnschedulableEventFulfiller
{
    private readonly IEventFulfiller<TExpected> _eventHandler;

    public TypeInversedUnschedulableEventObserver(IEventFulfiller<TExpected> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public void Fulfill<TActual>(EventFulfillmentContext context, TActual actualEventData)
    {
        if (actualEventData is null) {
            _eventHandler.Fulfill(context, default!);
            return;
        }

        if (actualEventData is not TExpected expectedEventData) {
            throw new InvalidOperationException(@$"The type of the actual event data does not match with the expected type
Actual type = {typeof(TExpected)}
Expected type = {typeof(TActual)}
");
        }

        _eventHandler.Fulfill(context, expectedEventData);
    }
}
