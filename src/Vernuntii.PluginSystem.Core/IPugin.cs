using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem;

/// <summary>
/// A plugin for <see cref="Vernuntii"/>.
/// </summary>
public interface IPlugin : IDisposable
{
    /// <summary>
    /// Order of plugin.
    /// </summary>
    int? Order { get; }

    /// <summary>
    /// Called when this plugin gets added. It gives
    /// you the opportunity to prepare dependencies.
    /// </summary>
    /// <param name="pluginRegistry"></param>
    void OnRegistration(IPluginRegistry pluginRegistry);

    /// <summary>
    /// Called when all plugins are registered and ordered.
    /// </summary>
    void OnCompletedRegistration();

    /// <summary>
    /// Called when this plugin gets notified about event aggregator.
    /// Called after <see cref="OnCompletedRegistration()"/>.
    /// </summary>
    /// <param name="eventAggregator"></param>
    void OnEventAggregation(IPluginEventAggregator eventAggregator);

    /// <summary>
    /// Called when plugin gets explictly destroyed.
    /// </summary>
    void OnDestroy();
}
