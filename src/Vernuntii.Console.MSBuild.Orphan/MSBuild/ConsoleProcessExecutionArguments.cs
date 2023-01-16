using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Vernuntii.Console.MSBuild;

internal sealed record ConsoleProcessExecutionArguments
{
    [AllowNull]
    public required string ConsoleExecutablePath {
        get => _consoleExecutablePath;

        init {
            if (value is null || value == "") {
                throw new ArgumentException("Console executable path is null or empty");
            }

            _consoleExecutablePath = value;
        }
    }

    [AllowNull]
    public required string ConfigPath {
        get => _configPath;

        init {
            if (value is null || value == "") {
                throw new ArgumentException("Config path is null or empty");
            }

            _configPath = value;
        }
    }

    public required string? Verbosity { get; init; }
    public required string? CacheId { get; init; }
    public required string? CacheCreationRetentionTime { get; init; }
    public required string? CacheLastAccessRetentionTime { get; init; }
    public required bool EmptyCaches { get; init; }

    public string Concenation =>
        _generatedArguments ??= CreateArguments();

    private string _consoleExecutablePath = null!;
    private string _configPath = null!;
    private string? _generatedArguments;

    private string CreateArguments()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"--config-path \"{ConfigPath}\"" +
            " --presentation-kind complex" +
            " --presentation-parts all" +
            " --presentation-view json");

        if (!string.IsNullOrWhiteSpace(Verbosity)) {
            stringBuilder.Append($" --verbosity {Verbosity}");
        }

        if (!string.IsNullOrWhiteSpace(CacheId)) {
            stringBuilder.Append($" --cache-id {CacheId}");
        }

        if (!string.IsNullOrWhiteSpace(CacheCreationRetentionTime)) {
            stringBuilder.Append($" --cache-creation-retention-time {CacheCreationRetentionTime}");
        }

        if (!string.IsNullOrWhiteSpace(CacheLastAccessRetentionTime)) {
            stringBuilder.Append($" --cache-last-access-retention-time {CacheLastAccessRetentionTime}");
        }

        if (EmptyCaches) {
            stringBuilder.Append($" --empty-caches");
        }

        return stringBuilder.ToString();
    }
}
