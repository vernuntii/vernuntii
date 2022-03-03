using System.Collections;
using System.Text.RegularExpressions;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a collection of branches.
    /// </summary>
    public class Branches : IReadOnlyList<IBranch>
    {
        private readonly List<IBranch> _branches;

        /// <inheritdoc />
        public int Count => _branches.Count;

        /// <summary>
        /// Creates an instance of <see cref="Branches" />.
        /// </summary>
        /// <param name="branches"></param>
        internal Branches(IEnumerable<IBranch> branches) => _branches = new List<IBranch>(branches);

        /// <inheritdoc />
        public IBranch this[int index] => _branches[index];

        /// <summary>
        /// Gets the branch by name.
        /// </summary>
        /// <param name="branchName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public IBranch? this[string branchName] {
            get {
                if (branchName == null) {
                    throw new ArgumentNullException(nameof(branchName));
                }

                if (branchName?.Length == 0) {
                    throw new ArgumentException("Branch name cannot be empty");
                }

                return _branches.SingleOrDefault(x => {
                    if (string.Equals(branchName, "HEAD", StringComparison.OrdinalIgnoreCase)) {
                        return branchName == x.ShortBranchName;
                    }

                    return RegexUtils.IsRegexPattern(branchName, out var pattern)
                        ? Regex.IsMatch(x.ShortBranchName, pattern) || Regex.IsMatch(x.LongBranchName, pattern)
                        : x.ShortBranchName == branchName || x.LongBranchName == branchName;
                });
            }
        }

        /// <inheritdoc />
        public IEnumerator<IBranch> GetEnumerator() => _branches.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
