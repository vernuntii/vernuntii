using Microsoft.Extensions.Logging;

namespace Vernuntii.Git
{
    public static class CommitVersionFinderFixture
    {
        public static readonly ILogger<CommitVersionFinder> DefaultCommitVersionFinderLogger = LoggerFactory.CreateLogger<CommitVersionFinder>();
    }
}
