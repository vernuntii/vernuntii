using System.Globalization;
using Microsoft.Extensions.Options;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    internal record BranchCasesOptions
    {
        private const string DefaultBranchCaseKey = "";

        private static List<RegexEscape> CreateDefaultPreReleaseEscapes() => new List<RegexEscape>() {
            new RegexEscape("/[A-Z]/", static value => value.ToLower(CultureInfo.InvariantCulture)),
            new RegexEscape("///", static _ => "-")
        };

        public IReadOnlyDictionary<string, IBranchCase> BranchCases => _branchCases;
        public IReadOnlyCollection<IRegexEscape>? DefaultPreReleaseEscapes { get; }

        Dictionary<string, IBranchCase> _branchCases = new Dictionary<string, IBranchCase>();

        public void AddBranchCase(IBranchCase branchCase)
        {
            if (branchCase is null) {
                throw new ArgumentNullException(nameof(branchCase));
            }

            string searchKey;

            if (string.IsNullOrEmpty(branchCase.IfBranch)) {
                searchKey = DefaultBranchCaseKey;
            } else {
                searchKey = branchCase.IfBranch;
            }

            if (_branchCases.ContainsKey(searchKey)) {
                throw new ArgumentException($"A branch case for \"{searchKey}\" has been already added");
            }

            _branchCases.Add(searchKey, branchCase);
        }

        internal class PostConfiguration : IPostConfigureOptions<BranchCasesOptions>
        {
            void IPostConfigureOptions<BranchCasesOptions>.PostConfigure(string _, BranchCasesOptions options)
            {
                if (options._branchCases.TryGetValue(DefaultBranchCaseKey, out var branchCase) && branchCase.PreReleaseEscapes == null) {
                    options._branchCases[DefaultBranchCaseKey] = new BranchCase(branchCase) with {
                        PreReleaseEscapes = CreateDefaultPreReleaseEscapes()
                    };
                }
            }
        }
    }
}
