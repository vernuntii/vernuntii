using System;

namespace Vernuntii.Configuration
{
    internal static class ConfigurationFilePaths
    {
        private static AnyPath RootDir = AppContext.BaseDirectory;

        internal static AnyPath FilesystemDir = RootDir / "filesystem";
        internal static AnyPath FileFinderDir = FilesystemDir / "file-finder";
        internal static AnyPath VersioningModeDir = FilesystemDir / "versioning-mode";
        internal static AnyPath MessageIndicatorsDir = FilesystemDir / "message-indicators";
    }
}
