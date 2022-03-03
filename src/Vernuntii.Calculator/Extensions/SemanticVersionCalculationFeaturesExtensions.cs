using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;

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
    }
}
