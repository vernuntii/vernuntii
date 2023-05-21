using Microsoft.Extensions.Logging;

namespace Vernuntii
{
    public static class LoggerFactory
    {
        private static readonly Microsoft.Extensions.Logging.LoggerFactory _loggerFactory = new();

        public static ILogger<T> CreateLogger<T>() => _loggerFactory.CreateLogger<T>();
    }
}
