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
