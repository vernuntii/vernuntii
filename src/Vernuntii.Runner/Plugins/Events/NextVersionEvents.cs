using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionPersistence;
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
        public static readonly EventDiscriminator<VersionPresentationContext> OnConfigureVersionPresentation = EventDiscriminator.New<VersionPresentationContext>();

        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> OnConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceProvider> OnCreatedScopedServiceProvider = EventDiscriminator.New<IServiceProvider>();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly EventDiscriminator CalculateNextVersion = EventDiscriminator.New();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly EventDiscriminator<IVersionCache> OnCalculatedNextVersion = EventDiscriminator.New<IVersionCache>();

        /// <summary>
        /// The event after the next version has been calculated by a command invocation.
        /// </summary>
        public static readonly EventDiscriminator<NextVersionPlugin.CommandInvocation> OnInvokeNextVersionCommand = EventDiscriminator.New<NextVersionPlugin.CommandInvocation>();

        /// <summary>
        /// The event after the next version has been calculated by a command invocation.
        /// </summary>
        public static readonly EventDiscriminator<NextVersionPlugin.CommandInvocationResult> OnInvokedNextVersionCommand = EventDiscriminator.New<NextVersionPlugin.CommandInvocationResult>();
    }
}
