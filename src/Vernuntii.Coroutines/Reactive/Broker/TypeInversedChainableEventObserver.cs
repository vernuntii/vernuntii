namespace Vernuntii.Reactive.Broker;

internal sealed class TypeInversedChainableEventObserver<TExpected> : ITypeInversedChainableEventObserver
{
    private readonly IEventObserver<TExpected> _eventObserver;

    public TypeInversedChainableEventObserver(IEventObserver<TExpected> eventObserver) =>
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));

    public void OnEmission<TActual>(EventEmissionBacklog emissionBacklog, TActual actualEventData)
    {
        if (actualEventData is null) {
            _eventObserver.OnEmission(emissionBacklog, default!);
            return;
        }

        if (actualEventData is not TExpected expectedEventData) {
            throw new InvalidOperationException(@$"The type of the actual event data does not match with the expected type
Actual type = {typeof(TExpected)}
Expected type = {typeof(TActual)}
");
        }

        _eventObserver.OnEmission(emissionBacklog, expectedEventData);
    }
}
