using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitTools.Testing
{
    internal class RepositoryBridge : IRepositoryBridge
    {
        IRepository _libGitRepository;
        Vernuntii.Git.IRepository _VernuntiiRepository;

        public RepositoryBridge(IRepository repository)
        {
            _VernuntiiRepository = new Vernuntii.Git.Repository(
                new Vernuntii.Git.RepositoryOptions() { GitDirectory = repository.Info.Path },
                LoggerFactory.Create(static delegate { }).CreateLogger<Vernuntii.Git.Repository>());

            _libGitRepository = repository;
        }

        public void Checkout(Tree tree, IEnumerable<string> paths, CheckoutOptions opts) => _libGitRepository.Checkout(tree, paths, opts);
        public void CheckoutPaths(string committishOrBranchSpec, IEnumerable<string> paths, CheckoutOptions checkoutOptions) => _libGitRepository.CheckoutPaths(committishOrBranchSpec, paths, checkoutOptions);
        public GitObject Lookup(ObjectId id) => _libGitRepository.Lookup(id);
        public GitObject Lookup(string objectish) => _libGitRepository.Lookup(objectish);
        public GitObject Lookup(ObjectId id, ObjectType type) => _libGitRepository.Lookup(id, type);
        public GitObject Lookup(string objectish, ObjectType type) => _libGitRepository.Lookup(objectish, type);
        public Commit Commit(string message, Signature author, Signature committer, CommitOptions options) => _libGitRepository.Commit(message, author, committer, options);
        public void Reset(ResetMode resetMode, Commit commit) => _libGitRepository.Reset(resetMode, commit);
        public void Reset(ResetMode resetMode, Commit commit, CheckoutOptions options) => _libGitRepository.Reset(resetMode, commit, options);
        public void RemoveUntrackedFiles() => _libGitRepository.RemoveUntrackedFiles();
        public RevertResult Revert(Commit commit, Signature reverter, RevertOptions options) => _libGitRepository.Revert(commit, reverter, options);
        public MergeResult Merge(Commit commit, Signature merger, MergeOptions options) => _libGitRepository.Merge(commit, merger, options);
        public MergeResult Merge(Branch branch, Signature merger, MergeOptions options) => _libGitRepository.Merge(branch, merger, options);
        public MergeResult Merge(string committish, Signature merger, MergeOptions options) => _libGitRepository.Merge(committish, merger, options);
        public MergeResult MergeFetchedRefs(Signature merger, MergeOptions options) => _libGitRepository.MergeFetchedRefs(merger, options);
        public CherryPickResult CherryPick(Commit commit, Signature committer, CherryPickOptions options) => _libGitRepository.CherryPick(commit, committer, options);
        public BlameHunkCollection Blame(string path, BlameOptions options) => _libGitRepository.Blame(path, options);
        public FileStatus RetrieveStatus(string filePath) => _libGitRepository.RetrieveStatus(filePath);
        public RepositoryStatus RetrieveStatus(StatusOptions options) => _libGitRepository.RetrieveStatus(options);
        public string Describe(Commit commit, DescribeOptions options) => _libGitRepository.Describe(commit, options);
        public void RevParse(string revision, out Reference reference, out GitObject obj) => _libGitRepository.RevParse(revision, out reference, out obj);

        public Branch Head => _libGitRepository.Head;

        public Configuration Config => _libGitRepository.Config;

        public LibGit2Sharp.Index Index => _libGitRepository.Index;

        public ReferenceCollection Refs => _libGitRepository.Refs;

        public IQueryableCommitLog Commits => _libGitRepository.Commits;

        public BranchCollection Branches => _libGitRepository.Branches;

        public TagCollection Tags => _libGitRepository.Tags;

        public RepositoryInformation Info => _libGitRepository.Info;

        public Diff Diff => _libGitRepository.Diff;

        public ObjectDatabase ObjectDatabase => _libGitRepository.ObjectDatabase;

        public NoteCollection Notes => _libGitRepository.Notes;

        public SubmoduleCollection Submodules => _libGitRepository.Submodules;

        public WorktreeCollection Worktrees => _libGitRepository.Worktrees;

        public Rebase Rebase => _libGitRepository.Rebase;

        public Ignore Ignore => _libGitRepository.Ignore;

        public Network Network => _libGitRepository.Network;

        public StashCollection Stashes => _libGitRepository.Stashes;

        public void Dispose() => _libGitRepository.Dispose();

        public string GetGitDirectory() => _VernuntiiRepository.GetGitDirectory();
        public Vernuntii.Git.IBranch GetActiveBranch() => _VernuntiiRepository.GetActiveBranch();
        public IEnumerable<Vernuntii.Git.ICommit> GetCommits(string? branchName = null, string? sinceCommit = null, bool reverse = false) => _VernuntiiRepository.GetCommits(branchName, sinceCommit, reverse);
        public IEnumerable<Vernuntii.Git.ICommitTag> GetCommitTags() => _VernuntiiRepository.GetCommitTags();
        public IReadOnlyList<Vernuntii.Git.CommitVersion> GetCommitVersions() => _VernuntiiRepository.GetCommitVersions();
        Vernuntii.Git.Branches Vernuntii.Git.IRepository.Branches => _VernuntiiRepository.Branches;
        public string? ExpandBranchName(string? branchName) => _VernuntiiRepository.ExpandBranchName(branchName);
    }
}
