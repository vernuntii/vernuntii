using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Lifecycle;

/// <summary>
/// The destruction aspect of <see cref="IPlugin"/>.
/// </summary>
public interface IPluginDestructionAspect
{
    /// <summary>
    /// Called to explictly destroy the plugin.
    /// </summary>
    ValueTask OnDestroy();
}
