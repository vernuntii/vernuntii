using System.Diagnostics.CodeAnalysis;
using Microsoft.Collections.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.Text.RegularExpressions;
using Teronis;

namespace Vernuntii.Extensions.BranchCases
{
    internal class BranchCasesProvider : IBranchCasesProvider
    {
        private const string DefaultBranchCaseKey = "";

        private static bool IsDefaultBranch(string branchName) =>
            string.IsNullOrEmpty(branchName);

        public IReadOnlyDictionary<string, IBranchCase> BranchCases =>
            _branchCasesOptions.Value.BranchCases;

        public IReadOnlyDictionary<string, IBranchCase> NestedBranchCases =>
            _nestedBranchCases ??= new Dictionary<string, IBranchCase>(
                _branchCasesOptions.Value.BranchCases.Where(p => !IsDefaultBranch(p.Key)));

        public IBranchCase ActiveBranchCase {
            get {
                SetMaybeBranchCaseArguments();
                return _activeBranchCaseArguments;
            }
        }

        public IBranchCase DefaultBranchCase {
            get {
                SetMaybeBranchCaseArguments();
                return _defaultBranchCaseArguments;
            }
        }

        private readonly IRepository _repository;
        private readonly SlimLazy<BranchCasesOptions> _branchCasesOptions;
        private readonly ILogger<BranchCasesProvider> _logger;
        private readonly Action<ILogger, string, Exception?> _logActiveBranchCase;
        private object? _lastActiveBranch;
        private IBranchCase? _defaultBranchCaseArguments;
        private IBranchCase? _activeBranchCaseArguments;
        private Dictionary<string, IBranchCase>? _nestedBranchCases;

        public BranchCasesProvider(
            IRepository repository,
            SlimLazy<IOptionsSnapshot<BranchCasesOptions>> branchCasesOptions,
            ILogger<BranchCasesProvider> logger)
        {
            _logActiveBranchCase = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(1),
                "Selected branch case {IfBranch}");

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            if (branchCasesOptions is null) {
                throw new ArgumentNullException(nameof(branchCasesOptions));
            }

            _branchCasesOptions = new SlimLazy<BranchCasesOptions>(() => branchCasesOptions.Value.Value);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void LogSelectedBranchCase(string? ifBranch) =>
            _logActiveBranchCase(_logger, string.IsNullOrEmpty(ifBranch) ? "\"\" (default)" : $"\"{ifBranch}\"", null);

        [MemberNotNull(
            nameof(_defaultBranchCaseArguments),
            nameof(_activeBranchCaseArguments))]
        private void SetMaybeBranchCaseArguments()
        {
            var branchCases = _branchCasesOptions.Value.BranchCases;
            var activeBranch = _repository.GetActiveBranch();

            if (Equals(_lastActiveBranch, activeBranch)
                && _defaultBranchCaseArguments is not null
                && _activeBranchCaseArguments is not null) {
                return;
            }

            if (branchCases.Count == 0) {
                throw new InvalidOperationException("A branch case has not been added");
            }

            OrderedDictionary<object, IBranchCase> caseArgumentsDictionary = new OrderedDictionary<object, IBranchCase>();

            foreach (var (branchName, caseArguments) in branchCases) {
                object searchKey;

                if (IsDefaultBranch(branchName)) {
                    searchKey = DefaultBranchCaseKey;
                } else {
                    // Returns null if not successful.
                    IBranch? GetBranchByExpandingName() => branchName switch {
                        var x when x == null || x == "" => null,
                        // Repository branch collection already supports regex
                        var x when RegexUtils.IsRegexPattern(x, out _) => null,
                        var x => _repository.ExpandBranchName(x) switch {
                            var y when y == null || y == "" => null,
                            var y => _repository.Branches[y!]
                        }
                    };

                    var branch = _repository.Branches[branchName] ?? GetBranchByExpandingName();

                    if (branch is null) {
                        continue;
                    }

                    searchKey = branch;
                }

                _ = caseArgumentsDictionary.Remove(searchKey);
                caseArgumentsDictionary[searchKey] = caseArguments;
            }

            if (!caseArgumentsDictionary.TryGetValue(DefaultBranchCaseKey, out var defaultBranchCaseArguments)) {
                throw new InvalidOperationException($"A default branch case has not been added");
            }

            if (!caseArgumentsDictionary.TryGetValue(activeBranch, out var activeBranchCaseArguments)) {
                activeBranchCaseArguments = defaultBranchCaseArguments;
            }

            LogSelectedBranchCase(activeBranchCaseArguments.IfBranch);
            _lastActiveBranch = activeBranch;
            _defaultBranchCaseArguments = defaultBranchCaseArguments;
            _activeBranchCaseArguments = activeBranchCaseArguments;
        }
    }
}
