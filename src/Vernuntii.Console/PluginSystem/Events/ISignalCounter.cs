namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Has the ability to evaluate the current count of signals.
    /// </summary>
    public interface ISignalCounter
    {
        /// <summary>
        /// The amount of signals.
        /// </summary>
        int SignalCount { get; }
    }
}
