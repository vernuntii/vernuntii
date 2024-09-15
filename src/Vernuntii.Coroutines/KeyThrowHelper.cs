namespace Vernuntii.Coroutines;

internal static class KeyThrowHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotSupportedException SchemaVersionNotSupported(Key key) =>
        new NotSupportedException($"The schema version of the key {key.SchemaVersion} is not supported");
}
