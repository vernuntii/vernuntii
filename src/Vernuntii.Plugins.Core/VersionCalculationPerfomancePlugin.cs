using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii
{
    /// <summary>
    /// Measures the time the application was alive.
    /// </summary>
    public class VersionCalculationPerfomancePlugin : Plugin
    {
        private Stopwatch _stopwatch = Stopwatch.StartNew();
        private ILogger<VersionCalculationPerfomancePlugin> _logger = null!;

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(LoggingEvents.EnabledLoggingInfrastructure, plugin => _logger = plugin.CreateLogger<VersionCalculationPerfomancePlugin>());

            Events.SubscribeOnce(NextVersionEvents.CalculatedNextVersion, version =>
                _logger.LogInformation("Loaded version {Version} in {LoadTime}", version, $"{_stopwatch.Elapsed.ToString("s\\.f", CultureInfo.InvariantCulture)}s"));
        }
    }
}
