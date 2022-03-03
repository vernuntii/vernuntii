namespace Vernuntii.Configuration.Shadowing
{
    /// <summary>
    /// Represents an shadowed configuration provider builder.
    /// </summary>
    public interface IShadowedConfigurationProviderBuilder : IShadowedConfigurationProviderBuilderConfigurator
    {
        /// <summary>
        /// Builds an shadowed configuration provider.
        /// </summary>
        IShadowedConfigurationProvider Build();
    }
}
