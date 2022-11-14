using System.Diagnostics.CodeAnalysis;
using CSF.Collections;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The base plugin descriptor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record PluginDescriptorBase<T>
        where T : IPlugin?
    {
        
    }
}
