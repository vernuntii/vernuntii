using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the global service collection.
    /// </summary>
    public interface IGlobalServicesPlugin : IPlugin, IServiceCollection
    {
    }
}
