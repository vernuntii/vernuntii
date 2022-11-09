using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Lifecycle;

/// <summary>
/// The registration aspect of <see cref="IPlugin"/>.
/// </summary>
public interface IPluginRegistrationAspect
{
    /// <summary>
    /// Called when this plugin gets added. It gives you the opportunity to prepare dependencies.
    /// </summary>
    /// <param name="pluginRegistry"></param>
    /// <returns><see langword="true"/> accepts the registration.</returns>
    ValueTask<bool> OnRegistration(IPluginRegistry pluginRegistry);
}
