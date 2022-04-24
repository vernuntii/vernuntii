using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ISemanticVersionCalculationFeatures"/>.
    /// </summary>
    public static class SemanticVersionCalculationFeaturesExtensions
    {
        private static void SetSemanticVersionCalculationOptionsMessagesProvider(ISemanticVersionCalculationFeatures features) =>
            features.ConfigureCalculationOptions(options => options
                .Configure<IMessagesProvider>((options, messagesProvider) => options.MessagesProvider = messagesProvider));

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        /// <param name="messagesProviderFactory"></param>
        public static ISemanticVersionCalculationFeatures UseMessagesProvider<T>(
            this ISemanticVersionCalculationFeatures extensions,
            Func<IServiceProvider, T> messagesProviderFactory)
            where T : class, IMessagesProvider
        {
            if (messagesProviderFactory is null) {
                throw new ArgumentNullException(nameof(messagesProviderFactory));
            }

            extensions.Services.TryAddSingleton<IMessagesProvider>(messagesProviderFactory);
            SetSemanticVersionCalculationOptionsMessagesProvider(extensions);
            return extensions;
        }

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        public static ISemanticVersionCalculationFeatures UseMessagesProvider<T>(this ISemanticVersionCalculationFeatures extensions)
            where T : class, IMessagesProvider
        {
            extensions.Services.TryAddSingleton<IMessagesProvider, T>();
            SetSemanticVersionCalculationOptionsMessagesProvider(extensions);
            return extensions;
        }

        /// <summary>
        /// Configures the instance of <see cref="SemanticVersionCalculationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptions"></param>
        public static ISemanticVersionCalculationFeatures ConfigureCalculationOptions(
            this ISemanticVersionCalculationFeatures extensions,
            Action<OptionsBuilder<SemanticVersionCalculationOptions>> configureOptions)
        {
            if (configureOptions is null) {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            configureOptions(extensions.Services.AddOptions<SemanticVersionCalculationOptions>());
            return extensions;
        }

        /// <summary>
        /// Overrides <see cref="SemanticVersionCalculationOptions.StartVersion"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="startVersion"></param>
        public static ISemanticVersionCalculationFeatures OverrideStartVersion(
            this ISemanticVersionCalculationFeatures features,
            SemanticVersion startVersion)
        {
            features.Services.AddOptions<SemanticVersionCalculationOptions>()
                .Configure(options => options.StartVersion = startVersion);

            return features;
        }

        /// <summary>
        /// Overrides start version provided by <paramref name="configuration"/>.
        /// It looks for the key <see cref="CalculatorConfigurationKeys.StartVersion"/>.
        /// If the value is null or empty start version won't be set.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentException"></exception>
        public static ISemanticVersionCalculationFeatures TryOverrideStartVersion(this ISemanticVersionCalculationFeatures features, IConfiguration configuration)
        {
            var startVersionString = configuration.GetValue(CalculatorConfigurationKeys.StartVersion, string.Empty);

            if (!string.IsNullOrEmpty(startVersionString)) {
                features.OverrideStartVersion(SemanticVersion.Parse(startVersionString));
            }

            return features;
        }
    }
}
