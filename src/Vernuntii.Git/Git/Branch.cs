using System.Diagnostics;

namespace Vernuntii.Git
{
    [DebuggerDisplay($"{{{nameof(ShortBranchName)}}}")]
    internal sealed class Branch : IBranch, IEquatable<Branch>
    {
        public string LongBranchName { get; }
        public string ShortBranchName { get; }
        public string CommitSha { get; }

        private readonly string _identifier;

        internal Branch(string commitSha, string longBranchName, string shortBranchName, string identifier)
        {
            _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            LongBranchName = longBranchName ?? throw new ArgumentNullException(nameof(longBranchName));
            ShortBranchName = shortBranchName ?? throw new ArgumentNullException(nameof(shortBranchName));
            CommitSha = commitSha ?? throw new ArgumentNullException(nameof(commitSha));
        }

        public bool Equals(Branch? other) => _identifier.Equals(other?._identifier, StringComparison.Ordinal);

        public override bool Equals(object? obj)
        {
            if (obj is Branch branch) {
                return Equals(branch);
            }

            return false;
        }

        bool IEquatable<IBranch>.Equals(IBranch? other) => Equals(other);

        public override int GetHashCode() => _identifier.GetHashCode(StringComparison.Ordinal);
    }
}
