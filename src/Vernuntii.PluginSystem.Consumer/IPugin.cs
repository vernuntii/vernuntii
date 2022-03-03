namespace Vernuntii.PluginSystem.Consumer;

/// <summary>
/// A plugin for <see cref="Vernuntii"/>.
/// </summary>
public interface IPlugin : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Order of plugin.
    /// </summary>
    int? Order { get; }

    /// <summary>
    /// Called when a new plugin is registered.
    /// </summary>
    /// <param name="plugin"></param>
    void OnPluginRegistered(IPlugin plugin);
}
