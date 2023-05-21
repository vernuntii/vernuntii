using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Extensions;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;

namespace Vernuntii.Git;

internal class VersionIncrementerTestSuite : IDisposable
{
    public ManualCommitHistory Commits { get; }

    private readonly ServiceProvider _serviceProvider;

    public VersionIncrementerTestSuite(VersionIncrementerTestSuiteOptions testOptions)
    {
        Commits = new ManualCommitHistory();

        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ICommitsAccessor>(Commits)
            .AddSingleton<ICommitVersionsAccessor>(Commits)
            .TakeViewOfVernuntii(vernuntii => vernuntii
                .AddVersionIncrementation(incrementation => incrementation
                    .UseMessagesProvider(sp => ActivatorUtilities.CreateInstance<GitCommitMessagesProvider>(sp, Commits))
                    .Configure(options => {
                        options.VersioningPreset = testOptions.VersioningPreset;
                    }))
                .TakeViewOfGit(git => git
                    .UseLatestCommitVersion()
                    .UseCommitMessagesProvider()
                    .Configure(preRelease => {
                        preRelease.SetSearchPreRelease(testOptions.SearchPreRelease);
                        preRelease.SetPostPreRelease(testOptions.PostPreRelease);
                    })))
            .BuildServiceProvider();
    }

    public ISemanticVersion GetIncrementedVersion()
    {
        using var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IVersionIncrementation>().GetIncrementedVersion();
    }

    public void Deconstruct(out ManualCommitHistory commits, out Func<ISemanticVersion> getIncrementedVersion)
    {
        commits = Commits;
        getIncrementedVersion = GetIncrementedVersion;
    }

    public void Dispose() =>
        _serviceProvider.Dispose();
}
