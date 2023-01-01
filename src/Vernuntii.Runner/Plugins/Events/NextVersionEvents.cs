using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.SemVer;
using Vernuntii.VersionPersistence.Presentation;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="INextVersionPlugin"/>.
    /// </summary>
    public static class NextVersionEvents
    {
        /// <summary>
        /// Configures the version presentation. 
        /// It is required to properly display the help text, but foremost being capable to visualize the next version.
        /// </summary>
        public static readonly EventDiscriminator<VersionPresentationContext> ConfigureVersionPresentation = EventDiscriminator.New<VersionPresentationContext>();

        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> ConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceProvider> CreatedScopedServiceProvider = EventDiscriminator.New<IServiceProvider>();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly EventDiscriminator<ISemanticVersion> CalculatedNextVersion = EventDiscriminator.New<ISemanticVersion>();


    }
}
