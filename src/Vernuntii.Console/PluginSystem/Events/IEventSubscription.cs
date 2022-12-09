namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Represents an event subscription that can be revoked by disposing it.
    /// </summary>
    public interface IEventSubscription : ISignalCounter, IDisposable
    {
        /// <summary>
        /// <see langword="true"/> if subscription has been disposed.
        /// </summary>
        bool IsDisposed { get; }
    }
}
