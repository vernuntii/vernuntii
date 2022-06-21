using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using YamlDotNet.Core.Tokens;
using Vernuntii.Caching;

namespace Vernuntii.Git.Command
{
    internal class CachingGitCommand : IGitCommand
    {
        internal IGitCommand UnderlyingCommand { get; }

        private readonly IMemoryCacheFactory _memoryCacheFactory;
        private IMemoryCache _memoryCache;

        public string WorkingDirectory => UnderlyingCommand.WorkingDirectory;

        public CachingGitCommand(IGitCommand gitCommand, IMemoryCacheFactory memoryCacheFactory)
        {
            UnderlyingCommand = gitCommand ?? throw new ArgumentNullException(nameof(gitCommand));
            _memoryCacheFactory = memoryCacheFactory ?? throw new ArgumentNullException(nameof(memoryCacheFactory));
            _memoryCache = _memoryCacheFactory.Create();
        }

        private T GetOrCreateCache<T>(ITuple key, Func<T> factory)
        {
            if (_memoryCache.TryGetValue(key, out T value)) {
                return value;
            }

            value = factory();
            _memoryCache.SetValue(key, value);
            return value;
        }

        private void UnsetCache(ITuple key) =>
            _memoryCache.UnsetValue(key);

        public string GetActiveBranchName() =>
            GetOrCreateCache(Tuple.Create(nameof(GetActiveBranchName)), UnderlyingCommand.GetActiveBranchName);

        public IEnumerable<IBranch> GetBranches() =>
            GetOrCreateCache(Tuple.Create(nameof(GetBranches)), () => UnderlyingCommand.GetBranches().ToList());

        public IEnumerable<ICommit> GetCommits(string? branchName, string? sinceCommit, bool reverse) =>
            GetOrCreateCache(Tuple.Create(nameof(GetCommits), branchName, sinceCommit, reverse), () => UnderlyingCommand.GetCommits(branchName, sinceCommit, reverse).ToList());

        public IEnumerable<ICommitTag> GetCommitTags() =>
            GetOrCreateCache(Tuple.Create(nameof(GetCommitTags)), () => UnderlyingCommand.GetCommitTags().ToList());

        public void UnsetCommitTagsCache() =>
            UnsetCache(Tuple.Create(nameof(GetCommitTags)));

        public string GetDotGitDirectory() =>
            GetOrCreateCache(Tuple.Create(nameof(GetDotGitDirectory)), UnderlyingCommand.GetDotGitDirectory);

        public bool IsHeadDetached() =>
            GetOrCreateCache(Tuple.Create(nameof(IsHeadDetached)), UnderlyingCommand.IsHeadDetached);

        public bool IsShallowRepository() =>
            GetOrCreateCache(Tuple.Create(nameof(IsShallowRepository)), UnderlyingCommand.IsShallowRepository);

        public bool TryResolveReference(string? name, ShowRefLimit showRefLimit, [NotNullWhen(true)] out IGitReference? reference)
        {
            var cache = GetOrCreateCache(Tuple.Create(nameof(TryResolveReference), name, showRefLimit), () => {
                var result = UnderlyingCommand.TryResolveReference(name, showRefLimit, out var reference);
                return Tuple.Create(result, reference);
            });

            reference = cache.Item2;
            return cache.Item1;
        }

        public void UnsetCache()
        {
            _memoryCache.Clear();
            Interlocked.Exchange(ref _memoryCache, _memoryCacheFactory.Create());
        }
    }
}
