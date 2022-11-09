using Microsoft.Extensions.Logging;
using Vernuntii.Logging;
using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// This plugin implements logging procedures.
    /// </summary>
    public interface ILoggingPlugin : IPlugin
    {
        /// <summary>
        /// The current verbosity.
        /// </summary>
        Verbosity Verbosity { get; }

        /// <summary>
        /// <see langword="true"/> means all log messages are written to stderr (default is <see langword="true"/>).
        /// </summary>
        bool WriteToStandardError { get; set; }

        /// <summary>
        /// Creates a logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        ILogger<T> CreateLogger<T>();

        /// <summary>
        /// Creates a logger.
        /// </summary>
        ILogger CreateLogger(string category);

        /// <summary>
        /// Binds internal logger of <see cref="ILoggingPlugin"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        void Bind(ILoggingBuilder builder);
    }
}
