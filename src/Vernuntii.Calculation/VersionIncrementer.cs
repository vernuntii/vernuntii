using Microsoft.Extensions.Logging;
using Vernuntii.MessagesProviders;
using Vernuntii.SemVer;
using Vernuntii.VersionIncrementing;
using Vernuntii.VersionTransformers;

namespace Vernuntii;

/// <summary>
/// Creates the commit messages reader that has the ability to calculate the next semantic version.
/// </summary>
public class VersionIncrementer : IVersionIncrementer
{
    private const string LogVersionTransformationTemplate = "Transformed version from {FromVersion} to {ToVersion}";

    private readonly ILogger _logger;
    private readonly Action<ILogger, ISemanticVersion, Exception?> _logInitialVersion;
    private readonly Action<ILogger, ISemanticVersion, ISemanticVersion, Exception?> _logVersionTransformation;
    private readonly Action<ILogger, ISemanticVersion, ISemanticVersion, Exception?> _logVersionPostTransformation;
    private readonly Action<ILogger, int, int, int, Exception?> _logMessageCountHavingProcessed;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">A logger (optional)</param>
    public VersionIncrementer(ILogger<VersionIncrementer> logger)
    {
        _logInitialVersion = LoggerMessage.Define<ISemanticVersion>(
            LogLevel.Information,
            new EventId(1, "StartVersion"),
            "Use start version {InitialVersion} for next transformation");

        _logVersionTransformation = LoggerMessage.Define<ISemanticVersion, ISemanticVersion>(
            LogLevel.Information,
            new EventId(2, "TransformedVersion"),
            LogVersionTransformationTemplate);

        _logVersionPostTransformation = LoggerMessage.Define<ISemanticVersion, ISemanticVersion>(
            LogLevel.Information,
            new EventId(4, "TransformedVersion"),
            "Post-transformed version from {FromVersion} to {ToVersion}");

        _logMessageCountHavingProcessed = LoggerMessage.Define<int, int, int>(
            LogLevel.Information,
            new EventId(5, "ProcessedMessageCount"),
            "Processed {MessageCount} messages (Transformations = {MessageCountInvolvedIntoTransformation}, Skipped = {MessageCountSkipped})");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private void LogInitialVersion(ISemanticVersion initialVersion) =>
        _logInitialVersion(_logger, initialVersion, null);

    private void LogVersionTransformation(IMessage message, ISemanticVersion fromVersion, ISemanticVersion toVersion)
    {
        if (message is IMessageProvidingDebugMessage messageProvidingDebugMessage && messageProvidingDebugMessage.DebugMessageFactory != null) {
            var debugMessage = messageProvidingDebugMessage.DebugMessageFactory();

            var arguments = new List<object>() { fromVersion, toVersion };
            arguments.AddRange(debugMessage.Arguments);

            // Disable:
            // 1. Template should be a static expression
#pragma warning disable CA2254
            _logger.LogInformation(new EventId(3), LogVersionTransformationTemplate +
                $" ({debugMessage.FormatString})", arguments.ToArray());
#pragma warning restore CA2254
        } else {
            _logVersionTransformation(_logger, fromVersion, toVersion, null);
        }
    }

    private void LogVersionPostTransformation(ISemanticVersion fromVersion, ISemanticVersion toVersion) =>
        _logVersionPostTransformation(_logger, fromVersion, toVersion, null);

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
    public ISemanticVersion IncrementVersion(VersionIncrementationOptions options)
    {
        var nextVersion = options.StartVersion;
        LogInitialVersion(nextVersion);

        var versionIncrementBuilder = new VersionIncrementBuilder();
        var versioningContext = new VersionIncrementContext(options);

        var messages = options.MessagesProvider?.GetMessages();
        var messageCounter = 0;
        var messageInvolvedIntoTransformationCounter = 0;

        if (messages is not null) {
            foreach (var message in messages) {
                messageCounter++;

                var preflightVersion = versionIncrementBuilder.BuildIncrement(message, versioningContext).TransformVersion(nextVersion);

                if (ReferenceEquals(preflightVersion, nextVersion) || preflightVersion.Equals(nextVersion)) {
                    // Version is unchanged.
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

        if (versioningContext.IsPreReleaseAdaptionOfCurrentVersionRequired && messageCounter > 0) {
            var currentVersion = nextVersion;
            nextVersion = versioningContext.PostVersionPreReleaseTransformer.TransformVersion(nextVersion);
            LogVersionPostTransformation(currentVersion, nextVersion);
        }

        LogMessageCountHavingProcessed(messageCounter, messageInvolvedIntoTransformationCounter);
        return nextVersion;
    }
}
