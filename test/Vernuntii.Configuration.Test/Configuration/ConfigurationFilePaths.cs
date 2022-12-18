namespace Vernuntii.Configuration
{
    internal static class ConfigurationFilePaths
    {
        private static readonly AnyPath RootDirectory = AppContext.BaseDirectory;

        internal static AnyPath FilesystemDirectory = RootDirectory / "filesystem";
        internal static AnyPath FileFinderDirectory = FilesystemDirectory / "file-finder";
        internal static AnyPath VersioningModeDirectory = FilesystemDirectory / "versioning-mode";
        internal static AnyPath MessageIndicatorsDirectory = FilesystemDirectory / "message-indicators";
    }
}
