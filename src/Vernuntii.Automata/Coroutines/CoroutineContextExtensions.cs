namespace Vernuntii.Coroutines;

internal static class CoroutineContextExtensions
{
    internal static void TreatAsNewChild(this ref CoroutineContext context)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ChildCoroutine;
    }

    internal static void TreatAsNewSibling(this ref CoroutineContext context, CoroutineContextBequesterOrigin additionalBequesterOrigin = CoroutineContextBequesterOrigin.RelativeCoroutine)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.SiblingCoroutine | additionalBequesterOrigin;
    }
}
