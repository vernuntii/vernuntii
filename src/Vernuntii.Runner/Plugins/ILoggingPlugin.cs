using Microsoft.Extensions.Logging;
using Vernuntii.Logging;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// This plugin implements logging procedures.
    /// </summary>
    public interface ILoggingPlugin : ILoggerFactory, IPlugin
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
        /// Binds internal logger of <see cref="ILoggingPlugin"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        void Bind(ILoggingBuilder builder);
    }
}
