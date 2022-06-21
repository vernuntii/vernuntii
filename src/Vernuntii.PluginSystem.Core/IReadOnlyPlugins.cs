namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents the read-only interface for accessing plugins.
    /// </summary>
    public interface IReadOnlyPlugins
    {
        /// <summary>
        /// Collection consisting of plugin registrations.
        /// </summary>
        IReadOnlyCollection<IPluginRegistration> PluginRegistrations { get; }

        /// <summary>
        /// The first appearing plugin that may or may not exist.
        /// </summary>
        ILazyPlugin<T> FirstLazy<T>() where T : IPlugin;

        /// <summary>
        /// The first appearing plugin.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        T First<T>() where T : IPlugin;
    }
}
