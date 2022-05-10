using Microsoft.Extensions.Logging;

namespace Vernuntii.Git
{
    internal static class TemporaryRepositoryFixture
    {
        public readonly static ILogger<TemporaryRepository> DefaultTemporaryRepositoryLogger = LoggerFactory.CreateLogger<TemporaryRepository>();
    }
}
