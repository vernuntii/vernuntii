namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

internal class EffectStore : AbstractEffectStore
{
    protected override Dictionary<EffectHandlerId, IEffectHandler> EffectHandlers { get; }

    public EffectStore()
    {
        var traceHandler = new TraceEffectHandler();
        var takeHandler = new TakeEffectHandler(traceHandler);

        EffectHandlers = new() {
            {  TraceEffectHandler.HandlerId, traceHandler },
            {  TakeEffectHandler.HandlerId, takeHandler }
        };
    }
}
