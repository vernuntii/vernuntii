namespace Vernuntii.Git
{
    /// <summary>
    /// Factory of a git repository.
    /// </summary>
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Creates a git repository.
        /// </summary>
        IRepository CreateRepository();
    }
}
