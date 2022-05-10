using Microsoft.Extensions.Logging;

namespace Vernuntii.Git
{
    public static class CommitVersionFinderFixture
    {
        public readonly static ILogger<CommitVersionFinder> DefaultCommitVersionFinderLogger = LoggerFactory.CreateLogger<CommitVersionFinder>();
    }
}
