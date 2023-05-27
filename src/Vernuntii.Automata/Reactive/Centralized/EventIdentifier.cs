namespace Vernuntii.Reactive.Centralized;

internal static class EventIdentifier
{
    private static ulong s_next;

    internal static ulong Next() => Interlocked.Increment(ref s_next);
}
