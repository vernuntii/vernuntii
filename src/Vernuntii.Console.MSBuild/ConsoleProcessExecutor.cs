using System;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Utilities;
using Teronis.Diagnostics;

namespace Vernuntii.Console
{
    internal class ConsoleProcessExecutor
    {
        private readonly string _consoleExecutablePath;
        private readonly TaskLoggingHelper _logger;

        public ConsoleProcessExecutor(string consoleExecutablePath, TaskLoggingHelper logger)
        {
            _consoleExecutablePath = consoleExecutablePath ?? throw new ArgumentNullException(nameof(consoleExecutablePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public SemanticVersionPresentation Execute(
            string? verbose,
            string configPath,
            string? cacheId,
            string? cacheCreationRetentionTime,
            string? cacheLastAccessRetentionTime,
            bool emptyCaches)
        {
            if (string.IsNullOrEmpty(configPath)) {
                throw new ArgumentException("Config path is null or empty");
            }

            var error = new StringBuilder();

            var startInfo = new ConsoleProcessStartInfo(_consoleExecutablePath,
                args: CreateConsoleArguments());

            var output = SimpleProcess.StartAndWaitForExitButReadOutput(
                startInfo,
                errorReceived: line => {
                    if (!string.IsNullOrWhiteSpace(line)) {
                        _logger.LogMessage(line);
                    }
                });

            return JsonSerializer.Deserialize<SemanticVersionPresentation>(output) ?? throw new InvalidOperationException();

            string CreateConsoleArguments()
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append($"--config-path \"{configPath}\"" +
                    " --presentation-kind complex" +
                    " --presentation-parts all" +
                    " --presentation-view json");

                if (!string.IsNullOrWhiteSpace(verbose)) {
                    stringBuilder.Append($" --verbose {verbose}");
                }

                if (!string.IsNullOrWhiteSpace(cacheId)) {
                    stringBuilder.Append($" --cache-id {cacheId}");
                }

                if (!string.IsNullOrWhiteSpace(cacheCreationRetentionTime)) {
                    stringBuilder.Append($" --cache-creation-retention-time {cacheCreationRetentionTime}");
                }

                if (!string.IsNullOrWhiteSpace(cacheLastAccessRetentionTime)) {
                    stringBuilder.Append($" --cache-last-access-retention-time {cacheLastAccessRetentionTime}");
                }

                if (emptyCaches) {
                    stringBuilder.Append($" --empty-caches");
                }

                return stringBuilder.ToString();
            }
        }
    }
}
