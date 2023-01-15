using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IVersionIncrementationServicesView"/>.
    /// </summary>
    public static class VersionIncrementationFeaturesExtensions
    {
        private static void SetMessagesProviderFromServiceProvider(IVersionIncrementationServicesView view) =>
            view.ConfigureOptions(options => options
                .Configure<IMessagesProvider>((options, messagesProvider) => options.MessagesProvider = messagesProvider));

        /// <summary>
        /// Uses a messages provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extensions"></param>
        /// <param name="messagesProviderFactory"></param>
        public static IVersionIncrementationServicesView UseMessagesProvider<T>(
            this IVersionIncrementationServicesView extensions,
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
        /// <param name="view"></param>
        public static IVersionIncrementationServicesView UseMessagesProvider<T>(this IVersionIncrementationServicesView view)
            where T : class, IMessagesProvider
        {
            view.Services.TryAddScoped<IMessagesProvider, T>();
            SetMessagesProviderFromServiceProvider(view);
            return view;
        }

        /// <summary>
        /// Configures the instance of <see cref="VersionIncrementationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptionsBuilder"></param>
        public static IVersionIncrementationServicesView ConfigureOptions(
            this IVersionIncrementationServicesView extensions,
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
        public static IVersionIncrementationServicesView Configure(
            this IVersionIncrementationServicesView extensions,
            Action<VersionIncrementationOptions> configureOptions) =>
            extensions.ConfigureOptions((OptionsBuilder<VersionIncrementationOptions> optionsBuilder) => optionsBuilder.Configure(configureOptions));

        /// <summary>
        /// Post-configures the instance of <see cref="VersionIncrementationOptions"/>.
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="configureOptions"></param>
        public static IVersionIncrementationServicesView PostConfigure(
            this IVersionIncrementationServicesView extensions,
            Action<VersionIncrementationOptions> configureOptions) =>
            extensions.ConfigureOptions((OptionsBuilder<VersionIncrementationOptions> optionsBuilder) => optionsBuilder.PostConfigure(configureOptions));

        /// <summary>
        /// Overrides <see cref="VersionIncrementationOptions.StartVersion"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="startVersion"></param>
        public static IVersionIncrementationServicesView OverrideStartVersion(
            this IVersionIncrementationServicesView view,
            SemanticVersion startVersion)
        {
            view.Services.AddOptions<VersionIncrementationOptions>()
                .Configure(options => options.StartVersion = startVersion);

            return view;
        }

        /// <summary>
        /// Overrides start version provided by <paramref name="configuration"/>.
        /// It looks for the key <see cref="CalculatorConfigurationKeys.StartVersion"/>.
        /// If the value is null or empty start version won't be set.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IVersionIncrementationServicesView TryOverrideStartVersion(this IVersionIncrementationServicesView view, IConfiguration configuration)
        {
            var startVersionString = configuration.GetValue(CalculatorConfigurationKeys.StartVersion, string.Empty);

            if (!string.IsNullOrEmpty(startVersionString)) {
                view.OverrideStartVersion(SemanticVersion.Parse(startVersionString));
            }

            return view;
        }
    }
}
