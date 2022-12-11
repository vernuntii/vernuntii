using Vernuntii.SemVer;

namespace Vernuntii.Git;

internal sealed class ManualCommitHistory : ICommitVersionsAccessor, ICommitsAccessor
{
    // Stacked commits
    private List<Commit> _commits = new();
    private HashSet<CommitVersion> _commitVersions = new(SemanticVersionComparer.VersionReleaseBuild);

    IEnumerable<ICommit> ICommitsAccessor.GetCommits(string? branchName, string? sinceCommit, bool fromOldToNew)
    {
        var commits = sinceCommit == null ? _commits : _commits.SkipWhile(commit => commit.Sha != sinceCommit).Skip(1);

        if (fromOldToNew) {
            return commits; // Oldest commit is equivlant to first inserted commit
        }

        // We have to reverse them to iterate from newest to oldest commits
        return commits.Reverse();
    }

    IReadOnlyCollection<ICommitVersion> ICommitVersionsAccessor.GetCommitVersions() =>
        _commitVersions;

    public ManualCommitHistory AddCommit(Commit commit)
    {
        _commits.Add(commit);
        return this;
    }

    public ManualCommitHistory AddCommit(string commitSubject) =>
        AddCommit(new Commit(Guid.NewGuid().ToString(), commitSubject));

    public ManualCommitHistory AddCommit(string commitSubject, ISemanticVersion version)
    {
        var commitSha = Guid.NewGuid().ToString();
        AddCommit(new Commit(commitSha, commitSubject));
        AddCommitVersion(new CommitVersion(commitSha, version));
        return this;
    }

    public ManualCommitHistory AddCommit(string commitSubject, SemanticVersion version) =>
        AddCommit(commitSubject, (ISemanticVersion)version);

    public ManualCommitHistory AddEmptyCommit()
    {
        var commitSha = Guid.NewGuid().ToString();
        AddCommit(new Commit(commitSha, commitSha));
        return this;
    }

    public ManualCommitHistory AddEmptyCommit(ISemanticVersion version)
    {
        var commitSha = Guid.NewGuid().ToString();
        var commitVersion = new CommitVersion(commitSha, version);
        AddCommit(new Commit(commitSha, commitVersion.ToString()));
        AddCommitVersion(commitVersion);
        return this;
    }

    public ManualCommitHistory AddEmptyCommit(SemanticVersion version) =>
        AddEmptyCommit((ISemanticVersion)version);

    public ManualCommitHistory AddCommitVersion(CommitVersion commitVersion)
    {
        _commitVersions.Add(commitVersion);
        return this;
    }
}
