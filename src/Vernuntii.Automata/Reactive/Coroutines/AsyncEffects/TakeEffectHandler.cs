namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal class TakeEffectHandler : IEffectHandler
{
    public readonly static EffectHandlerId HandlerId = new EffectHandlerId(typeof(TakeEffectHandler));

    private readonly TraceEffectHandler _handler;

    public TakeEffectHandler(TraceEffectHandler handler) =>
        _handler = handler;

    public async ValueTask HandleAsync(IEffect step)
    {
        if (step is not ITakeEffect takeStep) {
            throw new InvalidOperationException();
        }

        var connection = _handler.GetConnection(takeStep.Trace);
        await takeStep.HandleAsync(connection, CancellationToken.None);
    }
}
