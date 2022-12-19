namespace Vernuntii.Console.GlobalTool.NuGet
{
    /// <summary>
    /// Represents a package that is downloaded on a file system.
    /// </summary>
    /// <param name="Directory"></param>
    /// <param name="Version"></param>
    internal record DownloadedPackage(string Directory, string Version);
}
