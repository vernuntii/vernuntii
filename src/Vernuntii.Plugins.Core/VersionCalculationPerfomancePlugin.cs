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
        protected override void OnEventAggregation()
        {
            SubscribeEvent(LoggingEvents.EnabledLoggingInfrastructure.Discriminator, plugin => _logger = plugin.CreateLogger<VersionCalculationPerfomancePlugin>());

            SubscribeEvent(NextVersionEvents.CalculatedNextVersion.Discriminator, version =>
                _logger.LogInformation("Loaded version {Version} in {LoadTime}", version, $"{_stopwatch.Elapsed.ToString("s\\.f", CultureInfo.InvariantCulture)}s"));
        }
    }
}
