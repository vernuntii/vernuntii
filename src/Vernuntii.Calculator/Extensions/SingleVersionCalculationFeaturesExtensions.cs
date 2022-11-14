using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ISingleVersionCalculationFeatures"/>.
    /// </summary>
    public static class SingleVersionCalculationFeaturesExtensions
    {
        private static void SetVersionCalculationOptionsMessagesProvider(ISingleVersionCalculationFeatures features) =>
            features.ConfigureCalculationOptions(options => options
                .Configure<IMessagesProvider>((options, messagesProvider) => options.MessagesProvider = messagesProvider));

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        /// <param name="messagesProviderFactory"></param>
        public static ISingleVersionCalculationFeatures UseMessagesProvider<T>(
            this ISingleVersionCalculationFeatures extensions,
            Func<IServiceProvider, T> messagesProviderFactory)
            where T : class, IMessagesProvider
        {
            if (messagesProviderFactory is null) {
                throw new ArgumentNullException(nameof(messagesProviderFactory));
            }

            extensions.Services.TryAddSingleton<IMessagesProvider>(messagesProviderFactory);
            SetVersionCalculationOptionsMessagesProvider(extensions);
            return extensions;
        }

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        public static ISingleVersionCalculationFeatures UseMessagesProvider<T>(this ISingleVersionCalculationFeatures extensions)
            where T : class, IMessagesProvider
        {
            extensions.Services.TryAddSingleton<IMessagesProvider, T>();
            SetVersionCalculationOptionsMessagesProvider(extensions);
            return extensions;
        }

        /// <summary>
        /// Configures the instance of <see cref="SingleVersionCalculationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptions"></param>
        public static ISingleVersionCalculationFeatures ConfigureCalculationOptions(
            this ISingleVersionCalculationFeatures extensions,
            Action<OptionsBuilder<SingleVersionCalculationOptions>> configureOptions)
        {
            if (configureOptions is null) {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            configureOptions(extensions.Services.AddOptions<SingleVersionCalculationOptions>());
            return extensions;
        }

        /// <summary>
        /// Overrides <see cref="SingleVersionCalculationOptions.StartVersion"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="startVersion"></param>
        public static ISingleVersionCalculationFeatures OverrideStartVersion(
            this ISingleVersionCalculationFeatures features,
            SemanticVersion startVersion)
        {
            features.Services.AddOptions<SingleVersionCalculationOptions>()
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
        public static ISingleVersionCalculationFeatures TryOverrideStartVersion(this ISingleVersionCalculationFeatures features, IConfiguration configuration)
        {
            var startVersionString = configuration.GetValue(CalculatorConfigurationKeys.StartVersion, string.Empty);

            if (!string.IsNullOrEmpty(startVersionString)) {
                features.OverrideStartVersion(SemanticVersion.Parse(startVersionString));
            }

            return features;
        }
    }
}
