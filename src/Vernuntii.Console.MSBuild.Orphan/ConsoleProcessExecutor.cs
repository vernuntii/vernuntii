using System;
using System.Text;
using System.Text.Json;
using Kenet.SimpleProcess;
using Microsoft.Build.Utilities;

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

        public VersionPresentation Execute(
            string? verbosity,
            string configPath,
            string? cacheId,
            string? cacheCreationRetentionTime,
            string? cacheLastAccessRetentionTime,
            bool emptyCaches)
        {
            if (string.IsNullOrEmpty(configPath)) {
                throw new ArgumentException("Config path is null or empty");
            }

            var startInfo = new ConsoleProcessStartInfo(_consoleExecutablePath,
                args: CreateConsoleArguments());

            using var boundary = new ProcessBoundary();

            _ = new ProcessExecutorBuilder(startInfo)
                .WithExitCode(0)
                .WriteToBuffer(x => x.AddOutputWriter, out var outputBuffer, boundary)
                .AddErrorWriter(bytes => {
                    var line = Encoding.UTF8.GetString(bytes);

                    if (!string.IsNullOrWhiteSpace(line)) {
                        // Well, Vernuntii writes logger messages to error stream to
                        // make an distinction from the actual "next-version"-output,
                        // so do not write here to error stream!
                        _logger.LogMessage(line);
                    }
                })
                .RunToCompletion();

            var output = Encoding.UTF8.GetString(outputBuffer.WrittenSpan, outputBuffer.WrittenCount);

            try {
                return JsonSerializer.Deserialize<VersionPresentation>(output) ?? throw new InvalidOperationException();
            } catch {
                _logger.LogError(output);
                throw;
            }

            string CreateConsoleArguments()
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append($"--config-path \"{configPath}\"" +
                    " --presentation-kind complex" +
                    " --presentation-parts all" +
                    " --presentation-view json");

                if (!string.IsNullOrWhiteSpace(verbosity)) {
                    stringBuilder.Append($" --verbosity {verbosity}");
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
