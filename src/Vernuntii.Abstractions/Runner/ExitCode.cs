namespace Vernuntii.Runner
{
    /// <summary>
    /// Console exit code.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Sucess.
        /// </summary>
        Success = 0,
        /// <summary>
        /// Failure.
        /// </summary>
        Failure = 1,
        /// <summary>
        /// Commands line plugin was not running.
        /// </summary>
        NotExecuted = 2,
        /// <summary>
        /// Produced version already exists.
        /// </summary>
        VersionDuplicate = 3,
    }
}
