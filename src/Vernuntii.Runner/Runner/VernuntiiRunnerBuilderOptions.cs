namespace Vernuntii.Runner;

public record VernuntiiRunnerBuilderOptions
{
    public static readonly VernuntiiRunnerBuilderOptions Default = new VernuntiiRunnerBuilderOptions() {
        AddNextVersionRequirements = true
    };

    public static readonly VernuntiiRunnerBuilderOptions None = new VernuntiiRunnerBuilderOptions();

    public bool AddNextVersionRequirements { get; init; }

    /// <summary>
    /// If enabled, then the builder will check the existence of the "--daemonize"-flag as the first option in the console arguments.
    /// If the flag exists, then the Vernuntii runner will daemonize itself by starting a child process and terminating itsself.
    /// </summary>
    public bool CheckDaemonizeFlag { get; init; }
}
