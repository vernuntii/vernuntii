namespace Vernuntii.Coroutines;

internal static class CoroutineContextExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void TreatAsNewChild(this ref CoroutineContext context)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ChildCoroutine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void TreatAsNewSibling(this ref CoroutineContext context, CoroutineContextBequesterOrigin additionalBequesterOrigin = CoroutineContextBequesterOrigin.RelativeCoroutine)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.SiblingCoroutine | additionalBequesterOrigin;
    }

    internal static TService GetBequestedService<TService>(this in CoroutineContext coroutineContext, Key serviceKey) =>
        serviceKey.SchemaVersion switch {
            1 => (TService)coroutineContext.KeyedServicesToBequest[serviceKey],
            _ => throw KeyThrowHelper.SchemaVersionNotSupported(serviceKey)
        };
}
