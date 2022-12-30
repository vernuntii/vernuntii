namespace Vernuntii.Configuration.Queueing
{
    /// <summary>
    /// Represents an shadowed configuration provider builder.
    /// </summary>
    public interface IQueuedConfigurationProviderBuilder : IConfigurationProviderQueue
    {
        /// <summary>
        /// Builds an shadowed configuration provider.
        /// </summary>
        IQueuedConfigurationProvider Build();
    }
}
