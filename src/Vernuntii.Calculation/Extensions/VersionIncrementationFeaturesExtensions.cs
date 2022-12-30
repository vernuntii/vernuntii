using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IVersionIncrementationServicesScope"/>.
    /// </summary>
    public static class VersionIncrementationFeaturesExtensions
    {
        private static void SetMessagesProviderFromServiceProvider(IVersionIncrementationServicesScope scope) =>
            scope.ConfigureOptions(options => options
                .Configure<IMessagesProvider>((options, messagesProvider) => options.MessagesProvider = messagesProvider));

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        /// <param name="messagesProviderFactory"></param>
        public static IVersionIncrementationServicesScope UseMessagesProvider<T>(
            this IVersionIncrementationServicesScope extensions,
            Func<IServiceProvider, T> messagesProviderFactory)
            where T : class, IMessagesProvider
        {
            if (messagesProviderFactory is null) {
                throw new ArgumentNullException(nameof(messagesProviderFactory));
            }

            extensions.Services.TryAddScoped<IMessagesProvider>(messagesProviderFactory);
            SetMessagesProviderFromServiceProvider(extensions);
            return extensions;
        }

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        public static IVersionIncrementationServicesScope UseMessagesProvider<T>(this IVersionIncrementationServicesScope scope)
            where T : class, IMessagesProvider
        {
            scope.Services.TryAddScoped<IMessagesProvider, T>();
            SetMessagesProviderFromServiceProvider(scope);
            return scope;
        }

        /// <summary>
        /// Configures the instance of <see cref="VersionIncrementationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptionsBuilder"></param>
        public static IVersionIncrementationServicesScope ConfigureOptions(
            this IVersionIncrementationServicesScope extensions,
            Action<OptionsBuilder<VersionIncrementationOptions>> configureOptionsBuilder)
        {
            if (configureOptionsBuilder is null) {
                throw new ArgumentNullException(nameof(configureOptionsBuilder));
            }

            configureOptionsBuilder(extensions.Services.AddOptions<VersionIncrementationOptions>());
            return extensions;
        }

        /// <summary>
        /// Configures the instance of <see cref="VersionIncrementationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptions"></param>
        public static IVersionIncrementationServicesScope Configure(
            this IVersionIncrementationServicesScope extensions,
            Action<VersionIncrementationOptions> configureOptions) =>
            extensions.ConfigureOptions((OptionsBuilder<VersionIncrementationOptions> optionsBuilder) => optionsBuilder.Configure(configureOptions));

        /// <summary>
        /// Overrides <see cref="VersionIncrementationOptions.StartVersion"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="startVersion"></param>
        public static IVersionIncrementationServicesScope OverrideStartVersion(
            this IVersionIncrementationServicesScope features,
            SemanticVersion startVersion)
        {
            features.Services.AddOptions<VersionIncrementationOptions>()
                .Configure(options => options.StartVersion = startVersion);

            return features;
        }

        /// <summary>
        /// Overrides start version provided by <paramref name="configuration"/>.
        /// It looks for the key <see cref="CalculatorConfigurationKeys.StartVersion"/>.
        /// If the value is null or empty start version won't be set.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IVersionIncrementationServicesScope TryOverrideStartVersion(this IVersionIncrementationServicesScope scope, IConfiguration configuration)
        {
            var startVersionString = configuration.GetValue(CalculatorConfigurationKeys.StartVersion, string.Empty);

            if (!string.IsNullOrEmpty(startVersionString)) {
                scope.OverrideStartVersion(SemanticVersion.Parse(startVersionString));
            }

            return scope;
        }
    }
}
