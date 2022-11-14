using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem;

/// <summary>
/// A plugin.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Called when plugins have been constrcuted and are ready to serve.
    /// Called after registration but before destruction.
    /// </summary>
    /// <param name="events"></param>
    ValueTask OnExecution(IPluginEventCache events);
}
