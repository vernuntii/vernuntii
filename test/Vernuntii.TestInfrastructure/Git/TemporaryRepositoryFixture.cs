using Microsoft.Extensions.Logging;

namespace Vernuntii.Git
{
    internal static class TemporaryRepositoryFixture
    {
        public static readonly ILogger<TemporaryRepository> DefaultTemporaryRepositoryLogger = LoggerFactory.CreateLogger<TemporaryRepository>();
    }
}
