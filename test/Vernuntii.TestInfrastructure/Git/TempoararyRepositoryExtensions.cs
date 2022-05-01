namespace Vernuntii.Git
{
    internal static class TempoararyRepositoryExtensions
    {
        public static void CommitEmpty(this TempoararyRepository repository) =>
            repository.Commit(message: string.Empty, allowEmpty: true, allowEmptyMessage: true);
    }
}
