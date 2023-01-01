using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.Caching;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.MessagesProviders;
using Microsoft.Extensions.Configuration;
using Vernuntii.Extensions.BranchCases;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesView"/>.
    /// </summary>
    public static class GitServicesViewExtensions
    {
        private const string BranchesSectionKey = "Branches";

        /// <summary>
        /// Configures an instance of <see cref="IGitServicesView"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesView TakeViewOfGit(this IVernuntiiServicesView view, Action<IGitServicesView> configure)
        {
            var services = view.Services;

            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new GitServicesView(services);
            configure(options);
            return view;
        }

        /// <summary>
        /// Configures an instance of <see cref="IGitServicesView"/>.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView TakeViewOfGit(this IVernuntiiServicesView view) =>
            new GitServicesView(view.Services);

        /// <summary>
        /// Adds an instance of <see cref="Repository"/> as <see cref="IRepository"/> if <see cref="IRepository"/> has not been added before.
        /// Then <see cref="ICommitsAccessor"/>, <see cref="ICommitTagsAccessor"/> and <see cref="ICommitVersionsAccessor"/> are associated
        /// with this <see cref="IRepository"/> instance, in case any of these interfaces have not been added before.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView AddRepository(this IGitServicesView view)
        {
            var services = view.Services;
            services.TryAddSingleton<IGitDirectoryResolver>(GitDirectoryResolver.Default);
            services.TryAddSingleton<IGitCommand, GitCommand>();
            services.TryAddSingleton<IMemoryCacheFactory, DefaultMemoryCacheFactory>();
            services.TryAddScoped<IRepository, Repository>();
            services.TryAddScoped<ICommitsAccessor>(sp => sp.GetRequiredService<IRepository>());
            services.TryAddScoped<ICommitTagsAccessor>(sp => sp.GetRequiredService<IRepository>());
            services.TryAddScoped<ICommitVersionsAccessor>(sp => sp.GetRequiredService<IRepository>());
            services.TryAddScoped<IBranchesAccessor>(sp => sp.GetRequiredService<IRepository>());
            return view;
        }

        /// <summary>
        /// Configures the since-commit.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configure"></param>
        public static IGitServicesView Configure(this IGitServicesView view, Action<IGitConfigurer> configure)
        {
            var services = view.Services;
            GitConfigurer? configurer = null;

            Func<IServiceProvider, GitConfigurer> configurerProvider = sp => {
                if (configurer != null) {
                    return configurer;
                }

                configurer = new GitConfigurer(sp);
                configure(configurer);
                return configurer;
            };

            services.AddSingleton<IConfigureOptions<FoundCommitVersionOptions>>(sp => configurerProvider(sp));
            services.AddSingleton<IConfigureOptions<GitCommitMessagesProviderOptions>>(sp => configurerProvider(sp));
            services.AddScoped<IConfigureOptions<VersionIncrementationOptions>>(sp => configurerProvider(sp));
            return view;
        }

        /// <summary>
        /// Uses <see cref="GitCommitMessagesProvider"/> as <see cref="IMessagesProvider"/> and
        /// tries to register <see cref="GitCommitMessagesProviderOptions"/> from <see cref="IOptions{TOptions}"/>
        /// where T is <see cref="GitCommitMessagesProviderOptions"/>.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView UseCommitMessagesProvider(this IGitServicesView view)
        {
            view.Services.TryAddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<GitCommitMessagesProviderOptions>>().Value);

            view.Services.TakeViewOfVernuntii()
                .AddVersionIncrementation(view => view.UseMessagesProvider<GitCommitMessagesProvider>());

            return view;
        }

        /// <summary>
        /// Sets <see cref="VersionIncrementationOptions.StartVersion"/>
        /// to <see cref="FoundCommitVersion.CommitVersion"/> if the
        /// latest version has been found and is present in
        /// <see cref="FoundCommitVersion"/>.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView UseLatestCommitVersion(this IGitServicesView view)
        {
            var services = view.Services;
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<CommitVersionFinderOptions>>().Value);
            services.TryAddSingleton<ICommitVersionFinder, LatestCommitVersionFinder>();
            services.TryAddScoped<FoundCommitVersion>();

            services.AddOptions<GitCommitMessagesProviderOptions>()
                .PostConfigure<FoundCommitVersion, ICommitsAccessor>((options, foundCommitVersion, commitsAccessor) => {
                    if (foundCommitVersion.CommitVersion != null) {
                        // We want to start reading messages after found commit version
                        options.SinceCommit = foundCommitVersion.CommitVersion.CommitSha;
                    }
                });

            services.AddOptions<VersionIncrementationOptions>()
                .PostConfigure<FoundCommitVersion>((options, foundCommitVersion) => {
                    if (foundCommitVersion.CommitVersion != null) {
                        options.StartVersion = foundCommitVersion.CommitVersion;
                        options.IsStartVersionCoreAlreadyReleased = foundCommitVersion.IsCommitVersionCoreAlreadyReleased;
                    }
                });

            return view;
        }

        /// <summary>
        /// Uses <paramref name="configuration"/> through <paramref name="view"/>:
        /// <br/>- adds branch cases
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configuration"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesView UseConfigurationDefaults(
            this IGitServicesView view,
            IConfiguration configuration,
            Action<IBranchCase>? configureBranchCase = null) => view
                .AddBranchCases(configuration, configuration.GetSection(BranchesSectionKey).GetChildren(), configureBranchCase);
    }
}
