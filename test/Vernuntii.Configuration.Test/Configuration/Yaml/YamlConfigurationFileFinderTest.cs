using System.Collections.Generic;
using Xunit;

namespace Vernuntii.Configuration.Yaml;

public class YamlConfigurationFileFinderTest : IClassFixture<ConfigurationFixture>
{
    internal static AnyPath YmlDirectory = FileFinderDir / "yml";
    internal static AnyPath YmlEmptyDirectory = YmlDirectory / "empty";
    internal static FilePath YmlConfigFile = YmlDirectory + YamlConfigurationFileDefaults.YmlFileName;

    internal static AnyPath YamlDirectory = FileFinderDir / "yaml";
    internal static AnyPath YamlEmptyDirectory = YamlDirectory / "empty";
    internal static FilePath YamlConfigFile = YamlDirectory + YamlConfigurationFileDefaults.YamlFileName;

    private readonly ConfigurationFixture _configurationFixture;

    public YamlConfigurationFileFinderTest(ConfigurationFixture configurationFixture) =>
        _configurationFixture = configurationFixture;

    public static IEnumerable<object?[]> YamlFileFinderShouldFindFileGenerator()
    {
        //yield return new object?[] { YmlConfigFile, YmlEmptyDirectory, default(string) };
        { /* This superfluous block is required, otherwise I hit false positive in windows defender ._. */ }
        //yield return new object?[] { YmlConfigFile, YmlEmptyDirectory, YamlConfigurationFileDefaults.YmlFileName };

        yield return new object?[] { YamlConfigFile, YamlEmptyDirectory, default(string) };
        { /* This superfluous block is required, otherwise I hit false positive in windows defender ._. */ }
        //yield return new object?[] { YamlConfigFile, YamlEmptyDirectory, YamlConfigurationFileDefaults.YamlFileName };
    }

    [Theory]
    [MemberData(nameof(YamlFileFinderShouldFindFileGenerator))]
    public void YamlFileFinderShouldFindFile(string expectedConfigFile, string directoryPath, string? fileName)
    {
        AnyPath assumedFile = _configurationFixture.YamlConfigurationFileFinder
            .FindFile(directoryPath, fileName)
            .GetUpwardFilePath();

        Assert.Equal(expectedConfigFile, assumedFile);
    }
}

