using Microsoft.Extensions.Logging;
using Vernuntii.MessagesProviders;
using Vernuntii.MessageVersioning;
using Vernuntii.SemVer;

namespace Vernuntii;

/// <summary>
/// Creates the commit messages reader that has the ability to calculate the next semantic version.
/// </summary>
public class SemanticVersionCalculator : ISemanticVersionCalculator
{
    private readonly ILogger _logger;
    private readonly Action<ILogger, SemanticVersion, Exception?> _logInitialVersion;
    private readonly Action<ILogger, SemanticVersion, SemanticVersion, Exception?> _logVersionTransformation;
    private readonly Action<ILogger, SemanticVersion, SemanticVersion, Exception?> _logVersionPostTransformation;
    private readonly Action<ILogger, int, int, int, Exception?> _logMessageCountHavingProcessed;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">A logger (optional)</param>
    public SemanticVersionCalculator(ILogger<SemanticVersionCalculator> logger)
    {
        _logInitialVersion = LoggerMessage.Define<SemanticVersion>(
            LogLevel.Information,
            new EventId(1, "StartVersion"),
            "Use start version {InitialVersion} for next transformation");

        _logVersionTransformation = LoggerMessage.Define<SemanticVersion, SemanticVersion>(
            LogLevel.Information,
            new EventId(2, "TransformedVersion"),
            "Transformed version from {FromVersion} to {ToVersion}");

        _logVersionPostTransformation = LoggerMessage.Define<SemanticVersion, SemanticVersion>(
            LogLevel.Information,
            new EventId(4, "TransformedVersion"),
            "Post-transformed version from {FromVersion} to {ToVersion}");

        _logMessageCountHavingProcessed = LoggerMessage.Define<int, int, int>(
            LogLevel.Information,
            new EventId(5, "ProcessedMessageCount"),
            "Processed {MessageCount} messages (Transformations = {MessageCountInvolvedIntoTransformation}, Skipped = {MessageCountSkipped})");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private void LogInitialVersion(SemanticVersion initialVersion) =>
        _logInitialVersion(_logger, initialVersion, null);

    private void LogVersionTransformation(IMessage message, SemanticVersion fromVersion, SemanticVersion toVersion)
    {
        if (message is IMessageProvidingDebugMessage messageProvidingDebugMessage && messageProvidingDebugMessage.DebugMessageFactory != null) {
            var debugMessage = messageProvidingDebugMessage.DebugMessageFactory();

            var arguments = new List<object>() { fromVersion, toVersion };
            arguments.AddRange(debugMessage.Arguments);

            // Disable:
            // 1. Use the LoggerMessage delegates
            // 2. Template should be a static expression
#pragma warning disable CA1848, CA2254
            _logger.LogInformation(new EventId(3), "Transformed version from {FromVersion} to {ToVersion}" +
                $" ({debugMessage.FormatString})", arguments.ToArray());
#pragma warning restore CA1848, CA2254
        } else {
            _logVersionTransformation(_logger, fromVersion, toVersion, null);
        }
    }

    private void LogVersionPostTransformation(SemanticVersion fromVersion, SemanticVersion toVersion)
    {
        _logVersionPostTransformation(_logger, fromVersion, toVersion, null);
    }

    private void LogMessageCountHavingProcessed(int messageCount, int messageCountInvolvedIntoTransformation)
    {
        _logMessageCountHavingProcessed(
            _logger,
            messageCount,
            messageCountInvolvedIntoTransformation,
            messageCount - messageCountInvolvedIntoTransformation,
            null);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="options"><inheritdoc/></param>
    public SemanticVersion CalculateVersion(SemanticVersionCalculationOptions options)
    {
        var nextVersion = options.StartVersion;
        LogInitialVersion(nextVersion);

        var versionIncrementBuilder = new VersionIncrementBuilder(options.VersionIncrementOptions);
        var versioningContext = new MessageVersioningContext(options);

        var messages = options.MessagesProvider?.GetMessages();
        var messageCounter = 0;
        var messageInvolvedIntoTransformationCounter = 0;

        if (messages is not null) {
            foreach (var message in messages) {
                messageCounter++;

                var preflightVersion = nextVersion;

                foreach (var versionTransformer in versionIncrementBuilder.BuildIncrement(message, versioningContext)) {
                    if (versionTransformer is null || versionTransformer.DoesNotTransform) {
                        continue;
                    }

                    preflightVersion = versionTransformer.TransformVersion(preflightVersion);
                }

                if (ReferenceEquals(preflightVersion, nextVersion) || preflightVersion.Equals(nextVersion)) {
                    continue;
                }

                LogVersionTransformation(message, nextVersion, preflightVersion);
                nextVersion = preflightVersion;
                messageInvolvedIntoTransformationCounter++;

                // Renew versioning context.
                versioningContext = versioningContext with {
                    CurrentVersion = nextVersion
                };
            }
        }

        LogMessageCountHavingProcessed(messageCounter, messageInvolvedIntoTransformationCounter);

        if (options.CanPostTransform) {
            var currentVersion = nextVersion;
            nextVersion = options.PostTransformer.TransformVersion(nextVersion);
            LogVersionPostTransformation(currentVersion, nextVersion);
        }

        return nextVersion;
    }
}
