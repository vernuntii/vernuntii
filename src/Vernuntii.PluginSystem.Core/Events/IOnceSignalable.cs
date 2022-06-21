namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Represents one signal.
    /// </summary>
    public interface IOnceSignalable
    {
        /// <summary>
        /// <see langword="true"/> if this object has been signaled once.
        /// </summary>
        bool SignaledOnce { get; }
    }
}
