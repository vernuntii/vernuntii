using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

internal static class CoroutineContextExtensions
{
    internal static void Plus(this ref CoroutineContext parentContext, CoroutineContext additiveContext)
    {
        static (Dictionary<Key, object> Larger, IReadOnlyDictionary<Key, object> Smaller) CopyLargestDictionary(
            IReadOnlyDictionary<Key, object> parentContext,
            IReadOnlyDictionary<Key, object> additiveContext)
        {
            if (parentContext.Count >= additiveContext.Count) {
                var larger = new Dictionary<Key, object>(parentContext);
                return (larger, additiveContext);
            } else {
                var larger = new Dictionary<Key, object>(additiveContext);
                return (larger, parentContext);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void MergeIntoLeftDictionary(Dictionary<Key, object> left, IReadOnlyDictionary<Key, object> right)
        {
            foreach (var (key, value) in right) {
                left[key] = value;
            }
        }

        parentContext._keyedServices = additiveContext.KeyedServices;

        var (larger, smaller) = CopyLargestDictionary(parentContext.KeyedServicesToBequest, additiveContext.KeyedServicesToBequest);
        MergeIntoLeftDictionary(larger, smaller);
        parentContext._keyedServicesToBequest = larger;
    }

    internal static void TreatAsNewChild(this ref CoroutineContext context)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.ChildCoroutine;
    }

    internal static void TreatAsNewSibling(this ref CoroutineContext context)
    {
        context._keyedServices = null;
        context._bequesterOrigin = CoroutineContextBequesterOrigin.SiblingCoroutine;
    }
}
