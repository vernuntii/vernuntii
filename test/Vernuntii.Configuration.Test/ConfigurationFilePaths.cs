using System;

namespace Vernuntii.Configuration
{
    internal class ConfigurationFilePaths
    {
        private static AnyPath Root = AppContext.BaseDirectory;

        internal static AnyPath FilesystemDir = Root / "filesystem";
        internal static AnyPath FileFinderDir = FilesystemDir / "file-finder";
    }
}
