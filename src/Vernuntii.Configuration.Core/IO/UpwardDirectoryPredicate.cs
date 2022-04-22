namespace Vernuntii.Configuration.IO
{
    /// <summary>
    /// Decides whether directory info is correct.
    /// </summary>
    /// <param name="directoryInfo"></param>
    /// <returns>True if directoy info is correct.</returns>
    public delegate bool UpwardDirectoryPredicate(DirectoryInfo directoryInfo);
}
