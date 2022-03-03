using Microsoft.Extensions.Logging;

namespace Vernuntii
{
    public static class LoggerFactory
    {
        private readonly static Microsoft.Extensions.Logging.LoggerFactory _loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();

        public static ILogger<T> CreateLogger<T>() => _loggerFactory.CreateLogger<T>();
    }
}
