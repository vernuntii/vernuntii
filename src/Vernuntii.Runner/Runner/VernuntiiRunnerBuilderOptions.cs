namespace Vernuntii.Runner;

public record VernuntiiRunnerBuilderOptions
{
    public static readonly VernuntiiRunnerBuilderOptions Default = new VernuntiiRunnerBuilderOptions() {
        AddNextVersionRequirements = true
    };

    public static readonly VernuntiiRunnerBuilderOptions None = new VernuntiiRunnerBuilderOptions();

    public bool AddNextVersionRequirements { get; init; }
}
