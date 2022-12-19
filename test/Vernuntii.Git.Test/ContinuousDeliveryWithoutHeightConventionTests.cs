using FluentAssertions;
using Vernuntii.Git;
using Vernuntii.HeightConventions;
using Vernuntii.SemVer;
using Vernuntii.VersioningPresets;
using Vernuntii.VersionTransformers;
using Xunit;

namespace Vernuntii;

public class ContinuousDeliveryWithoutHeightConventionTests
{
    private static readonly IVersioningPreset s_continuousDeliveryWithoutHeightConvention = VersioningPreset.ContinuousDelivery with { HeightConvention = HeightConvention.None };

    [Fact]
    public void Pre_release_should_become_release_and_reuse_version_core()
    {
        using var testSuite = new VersionIncrementerTestSuite(new VersionIncrementerTestSuiteOptions(s_continuousDeliveryWithoutHeightConvention) {
            SearchPreRelease = "alpha",
            PostPreRelease = ""
        });

        testSuite.Commits
            .AddEmptyCommit(SemanticVersion.OnePatch.With.PreRelease("alpha").ToVersion(out var last))
            .AddEmptyCommit();

        var nextVersion = testSuite.GetIncrementedVersion();
        nextVersion.Should().Be(last.WithOnlyCore().ToVersion());
    }

    [Fact]
    public void Release_should_become_pre_release_with_bumped_patch()
    {
        using var testSuite = new VersionIncrementerTestSuite(new VersionIncrementerTestSuiteOptions(s_continuousDeliveryWithoutHeightConvention) {
            PostPreRelease = "alpha"
        });

        testSuite.Commits
            .AddEmptyCommit(SemanticVersion.OnePatch.With.ToVersion(out var last))
            .AddEmptyCommit();

        var nextVersion = testSuite.GetIncrementedVersion();
        nextVersion.Should().Be(last.IncrementPatch().PreRelease("alpha").ToVersion());
    }

    [Fact]
    public void Release_should_not_change()
    {
        using var testSuite = new VersionIncrementerTestSuite(new VersionIncrementerTestSuiteOptions(s_continuousDeliveryWithoutHeightConvention) {
            PostPreRelease = "",
        });

        testSuite.Commits.AddEmptyCommit(SemanticVersion.OnePatch.With.ToVersion(out var last));

        var nextVersion = testSuite.GetIncrementedVersion();
        Assert.Equal(last.ToVersion(), nextVersion);
    }

    [Fact]
    public void Pre_release_should_remain_pre_release_with_bumped_patch()
    {
        using var testSuite = new VersionIncrementerTestSuite(new VersionIncrementerTestSuiteOptions(s_continuousDeliveryWithoutHeightConvention) {
            PostPreRelease = "alpha",
        });

        testSuite.Commits
            .AddEmptyCommit(SemanticVersion.OnePatch.With.PreRelease("alpha").ToVersion(out var last))
            .AddEmptyCommit();

        var nextVersion = testSuite.GetIncrementedVersion();
        nextVersion.Should().Be(last.IncrementPatch().ToVersion());
    }

    [Fact]
    public void Pre_release_should_remain_pre_release_but_with_adapted_pre_release()
    {
        using var testSuite = new VersionIncrementerTestSuite(new VersionIncrementerTestSuiteOptions(s_continuousDeliveryWithoutHeightConvention) {
            PostPreRelease = "beta",
        });

        testSuite.Commits
            .AddEmptyCommit(SemanticVersion.OnePatch.With.PreRelease("alpha").ToVersion(out var last))
            .AddEmptyCommit();

        var nextVersion = testSuite.GetIncrementedVersion();
        nextVersion.Should().Be(last.PreRelease("beta").ToVersion());
    }
}
