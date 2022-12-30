namespace Vernuntii.Configuration.Json;

public class JsonConfigurationFileFinderTest : IClassFixture<ConfigurationFixture>
{
    internal static AnyPath JsonDirectory = FileFinderDirectory / "json";
    internal static AnyPath JsonEmptyDirectory = JsonDirectory / "empty";
    internal static FilePath JsonConfigFile = JsonDirectory + JsonConfigurationFileDefaults.JsonFileName;

    private readonly ConfigurationFixture _configurationFixture;

    public JsonConfigurationFileFinderTest(ConfigurationFixture configurationFixture) =>
        _configurationFixture = configurationFixture;

    public static IEnumerable<object?[]> GenerateJsonConfigurationFileSearchData()
    {
        yield return new object?[] { JsonEmptyDirectory, default(string) };
        yield return new object?[] { JsonEmptyDirectory, JsonConfigurationFileDefaults.JsonFileName };
    }

    [Theory]
    [MemberData(nameof(GenerateJsonConfigurationFileSearchData))]
    public void JsonFileFinderShouldFindFile(string directoryPath, string? fileName)
    {
        AnyPath assumedFile = _configurationFixture.JsonConfigurationFileFinder
            .FindFile(directoryPath, fileName)
            .GetHigherLevelFilePath();

        Assert.Equal(JsonConfigFile, assumedFile);
    }
}
