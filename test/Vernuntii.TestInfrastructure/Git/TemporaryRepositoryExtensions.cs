namespace Vernuntii.Git
{
    internal static class TemporaryRepositoryExtensions
    {
        public static void CommitEmpty(this TemporaryRepository repository) =>
            repository.Commit(message: string.Empty, allowEmpty: true, allowEmptyMessage: true);
    }
}
