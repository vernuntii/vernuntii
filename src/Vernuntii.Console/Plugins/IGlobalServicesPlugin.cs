using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the global service collection.
    /// </summary>
    public interface IGlobalServicesPlugin : IPlugin, IServiceCollection
    {
    }
}
