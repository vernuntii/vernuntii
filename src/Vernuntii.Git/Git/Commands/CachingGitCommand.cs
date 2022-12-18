using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vernuntii.Caching;

namespace Vernuntii.Git.Commands
{
    internal sealed class CachingGitCommand : IGitCommand
    {
        public static readonly ITuple GetCommitTagsCacheKey = Tuple.Create(nameof(GetCommitTags));

        internal IGitCommand UnderlyingCommand { get; }

        public string WorkingTreeDirectory => UnderlyingCommand.WorkingTreeDirectory;

        private readonly IMemoryCache _memoryCache;

        public CachingGitCommand(IGitCommand gitCommand, IMemoryCache memoryCache)
        {
            UnderlyingCommand = gitCommand ?? throw new ArgumentNullException(nameof(gitCommand));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        private T GetOrCreateCache<T>(ITuple key, Func<T> factory)
        {
            if (_memoryCache.TryGetCache(key, out T value)) {
                return value;
            }

            value = factory();
            _memoryCache.SetCache(key, value);
            return value;
        }

        private void UnsetCache(ITuple key) =>
            _memoryCache.UnsetCache(key);

        public string GetActiveBranchName() =>
            GetOrCreateCache(Tuple.Create(nameof(GetActiveBranchName)), UnderlyingCommand.GetActiveBranchName);

        public IEnumerable<IBranch> GetBranches() =>
            GetOrCreateCache(Tuple.Create(nameof(GetBranches)), () => UnderlyingCommand.GetBranches().ToList());

        public IEnumerable<ICommit> GetCommits(string? branchName, string? sinceCommit, bool reverse) =>
            GetOrCreateCache(Tuple.Create(nameof(GetCommits), branchName, sinceCommit, reverse), () => UnderlyingCommand.GetCommits(branchName, sinceCommit, reverse).ToList());

        public IEnumerable<ICommitTag> GetCommitTags() =>
            GetOrCreateCache(GetCommitTagsCacheKey, () => UnderlyingCommand.GetCommitTags().ToList());

        public string GetGitDirectory() =>
            GetOrCreateCache(Tuple.Create(nameof(GetGitDirectory)), UnderlyingCommand.GetGitDirectory);

        public bool IsHeadDetached() =>
            GetOrCreateCache(Tuple.Create(nameof(IsHeadDetached)), UnderlyingCommand.IsHeadDetached);

        public bool IsShallow() =>
            GetOrCreateCache(Tuple.Create(nameof(IsShallow)), UnderlyingCommand.IsShallow);

        public bool TryResolveReference(string? name, ShowRefLimit showRefLimit, [NotNullWhen(true)] out IGitReference? reference)
        {
            var cache = GetOrCreateCache(Tuple.Create(nameof(TryResolveReference), name, showRefLimit), () => {
                var result = UnderlyingCommand.TryResolveReference(name, showRefLimit, out var reference);
                return Tuple.Create(result, reference);
            });

            reference = cache.Item2;
            return cache.Item1;
        }

        public void UnsetCache() =>
            _memoryCache.UnsetCache();

        public void Dispose() =>
            UnderlyingCommand.Dispose();
    }
}
