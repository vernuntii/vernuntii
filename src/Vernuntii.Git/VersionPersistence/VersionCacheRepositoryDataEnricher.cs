using Vernuntii.Git;

namespace Vernuntii.VersionPersistence;

internal class VersionCacheRepositoryDataEnricher : IVersionCacheDataTuplesEnricher
{
    private readonly IRepository _repository;

    public VersionCacheRepositoryDataEnricher(IRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public void Enrich(VersionCacheDataTuples dataTuples)
    {
        var activeBranch = _repository.GetActiveBranch();
        dataTuples.AddData(GitVersionCacheParts.BranchName, activeBranch.ShortBranchName);
        dataTuples.AddData(GitVersionCacheParts.BranchTip, activeBranch.CommitSha);
    }
}
