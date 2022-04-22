using Microsoft.Extensions.Logging;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// This plugin implements logging procedures.
    /// </summary>
    public interface ILoggingPlugin : IPlugin
    {
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
        /// Binds <see cref="ILoggingPlugin"/> internal logger to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"></param>
        void Bind(ILoggingBuilder builder);
    }
}
