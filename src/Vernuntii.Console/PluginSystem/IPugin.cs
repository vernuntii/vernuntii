using Vernuntii.PluginSystem.Reactive;

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
    Task OnExecution(IEventSystem events);
}
