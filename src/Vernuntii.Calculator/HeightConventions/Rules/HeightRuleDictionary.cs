using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Collections.Extensions;

namespace Vernuntii.HeightConventions.Rules
{
    /// <summary>
    /// Contains instances of <see cref="IHeightRule"/>.
    /// </summary>
    public sealed class HeightRuleDictionary : IHeightRuleDictionary
    {
        /// <summary>
        /// Empty dictionary without any rules.
        /// </summary>
        public readonly static HeightRuleDictionary Empty = new HeightRuleDictionary();

        /// <summary>
        /// Default rules for one dotted string.
        /// </summary>
        public readonly static HeightRuleDictionary BehindFirstDotRules = new HeightRuleDictionary(new[] {
            new HeightRule(0, "{}."),
            new HeightRule(1, "{0}.{y}")
        });

        /// <inheritdoc/>
        public IEnumerable<int> Keys => ((IReadOnlyDictionary<int, IHeightRule>)_rules).Keys;
        /// <inheritdoc/>
        public int Count => _rules.Count;

        private OrderedDictionary<int, IHeightRule> _rules = new OrderedDictionary<int, IHeightRule>();

        private HeightRuleDictionary()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="rules"></param>
        public HeightRuleDictionary(IEnumerable<IHeightRule> rules)
        {
            foreach (var rule in rules) {
                _rules.Add(rule.IfDots, rule);
            }
        }

        /// <inheritdoc/>
        public IHeightRule this[int key] => _rules[key];

        /// <inheritdoc/>
        public bool ContainsKey(int key) => _rules.ContainsKey(key);

        /// <inheritdoc/>
        public bool TryGetValue(int key, [MaybeNullWhen(false)] out IHeightRule value) =>
            _rules.TryGetValue(key, out value);

        /// <inheritdoc/>
        public IEnumerable<IHeightRule> Values =>
            ((IReadOnlyDictionary<int, IHeightRule>)_rules).Values;

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<int, IHeightRule>> GetEnumerator() =>
            ((IEnumerable<KeyValuePair<int, IHeightRule>>)_rules).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)_rules).GetEnumerator();
    }
}
