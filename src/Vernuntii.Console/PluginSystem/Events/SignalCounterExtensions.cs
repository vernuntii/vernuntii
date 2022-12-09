namespace Vernuntii.PluginSystem.Events;

public static class SignalCounterExtensions
{
    /// <summary>
    /// Checks if the signal counter has been incremented at least once.
    /// </summary>
    /// <param name="counter"></param>
    /// <returns>
    /// <see langword="true"/> if the signal counter is greater than zero.
    /// </returns>
    public static bool IsOnceSignaled(this ISignalCounter? counter) =>
        counter != null && counter.SignalCount > 0;
}
