namespace Vernuntii.PluginSystem.Reactive;

internal sealed class EventObserver<TExpected> : IEventObserver
{
    private readonly IEventObserver<TExpected> _eventHandler;

    public EventObserver(IEventObserver<TExpected> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public void OnFulfillment<TActual>(EventFulfillmentContext context, TActual actualEventData)
    {
        if (actualEventData is null) {
            _eventHandler.OnFulfilled(context, default!);
            return;
        }

        if (actualEventData is not TExpected expectedEventData) {
            throw new InvalidOperationException(@$"The type of the actual event data does not match with the expected type
Actual type = {typeof(TExpected)}
Expected type = {typeof(TActual)}
");
        }

        _eventHandler.OnFulfilled(context, expectedEventData);
    }
}
