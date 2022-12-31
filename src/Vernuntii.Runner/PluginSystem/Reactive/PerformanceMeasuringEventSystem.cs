using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Vernuntii.Diagnostics;

namespace Vernuntii.PluginSystem.Reactive;

internal sealed record PerformanceMeasuringEventSystem : EventSystem
{
    private static string DepthIndicator(int actualDepth, bool open)
    {
        var visualDepth = actualDepth + 1;
        var stringBuilder = new StringBuilder(visualDepth);

        if (visualDepth > 1) {
            if (open) {
                stringBuilder.Append("┐ ");
            } else {
                stringBuilder.Append("┘ ");
            }
        }

        if (visualDepth > 2) {
            stringBuilder.Insert(0, "├");
        }

        if (visualDepth > 3) {
            stringBuilder.Insert(0, new string('│', visualDepth - 3));
        }

        return stringBuilder.ToString();
    }

    private static string PreviousDepthsTimes(IReadOnlyDictionary<int, Stopwatch> _depthRelativeWatches, int actualDepth, bool withComma)
    {
        if (actualDepth <= 1) {
            return "";
        }

        var currentDepth = actualDepth - 1;
        var currentNegativeDepth = 0;
        var stringBuider = new StringBuilder();

        if (withComma) {
            stringBuider.Append(", ");
        }

        do {
            if (currentNegativeDepth != 0) {
                stringBuider.Append(", ");
            }

            currentNegativeDepth--;
            var currentDepthWatch = _depthRelativeWatches[currentDepth];
            stringBuider.Append(currentDepthWatch.Elapsed.ToSecondsString());
        } while (currentDepth-- != 1);

        return stringBuider.ToString();
    }

    private ILogger<EventSystem> _logger;
    private int _currentDepth;
    private ConcurrentDictionary<int, Stopwatch> _depthRelativeWatches = new();

    internal PerformanceMeasuringEventSystem(ILogger<EventSystem> logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    internal override async Task FulfillScheduledEventsAsync<T>(object eventId, T eventData, EventFulfillmentContext fulfillmentContext)
    {
        var depth = Interlocked.Increment(ref _currentDepth);
        var watch = new Stopwatch();
        watch.Start();
        _depthRelativeWatches.AddOrUpdate(depth, static (_, watch) => watch, static (_, _, watch) => watch, watch);

        _logger.LogTrace(
            DepthIndicator(depth, true) + "Fulfill event {EventId}<{EventType}>",
            eventId,
            typeof(T).Name);

        await base.FulfillScheduledEventsAsync(eventId, eventData, fulfillmentContext);

        _logger.LogTrace(
            DepthIndicator(depth, false) + "Fulfilled event {EventId}<{EventType}> in {FulfillmentTime}{PreviousDepthsTimes}",
            eventId,
            typeof(T).Name,
            watch.Elapsed.ToSecondsString(),
            PreviousDepthsTimes(_depthRelativeWatches, depth, withComma: true));

        Interlocked.Decrement(ref _currentDepth);
    }
}
