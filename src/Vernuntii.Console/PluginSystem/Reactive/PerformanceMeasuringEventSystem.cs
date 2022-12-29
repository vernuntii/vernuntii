using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Vernuntii.PluginSystem.Reactive;

internal class PerformanceMeasuringEventSystem : EventSystem
{
    static string DepthIndicator(int depth, bool open)
    {
        var stringBuilder = new StringBuilder(depth);

        if (depth > 1)
        {
            if (open)
            {
                stringBuilder.Append("┐ ");
            }
            else
            {
                stringBuilder.Append("┘ ");
            }
        }

        if (depth > 2)
        {
            stringBuilder.Insert(0, '│');
        }

        if (depth > 3)
        {
            stringBuilder.Insert(0, new string(' ', depth - 3));
        }

        return stringBuilder.ToString();
    }

    ILogger<EventSystem> _logger;
    int _depth;
    Stopwatch _sinceLastFulfillmentWatch = new();

    public PerformanceMeasuringEventSystem(ILogger<EventSystem> logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override async Task FullfillAsync<T>(ulong eventId, T eventData)
    {
        var depth = Interlocked.Increment(ref _depth);
        var watch = new Stopwatch();

        watch.Start();_sinceLastFulfillmentWatch.Start();
        var timeSinceLastFulfillment = _sinceLastFulfillmentWatch.Elapsed;

        _logger.LogTrace(
            DepthIndicator(depth, true) + "Fulfill event {EventId}:{EventType} ({TimeSincePreviousFulfillment} since previous)",
            eventId,
            typeof(T).Name,
            $"{timeSinceLastFulfillment.ToString("s\\.ff", CultureInfo.InvariantCulture)}s");

        await base.FullfillAsync(eventId, eventData);
        _sinceLastFulfillmentWatch.Restart();

        _logger.LogTrace(
            DepthIndicator(depth, false) + "Fulfilled event {EventId}:{EventType} in {FulfillmentTime}",
            eventId,
            typeof(T).Name,
            $"{watch.Elapsed.ToString("s\\.ff", CultureInfo.InvariantCulture)}s");

        Interlocked.Decrement(ref _depth);
    }
}
