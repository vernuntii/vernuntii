using System.Collections;
using Microsoft.Extensions.Configuration;
using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Configuration.Shadowing
{
    /// <summary>
    /// Can overshadow an empty property by an empty entry.
    /// </summary>
    internal class EmptyArrayCorrectionConfigurationProvider : ConfigurationProvider, IEnumerable<string>
    {
        private readonly List<string?> _parentPaths = new();
        private readonly IConfigurationProvider _originConfigurationProvider;

        public EmptyArrayCorrectionConfigurationProvider(IConfigurationProvider originConfigurationProvider) =>
            _originConfigurationProvider = originConfigurationProvider ?? throw new ArgumentNullException(nameof(originConfigurationProvider));

        /// <summary>
        /// Adds another parent path.
        /// </summary>
        /// <param name="parentPath">Parent path. RegEx is allowed when surrounded by '/'.</param>
        public void Add(string? parentPath) =>
            _parentPaths.Add(parentPath);

        private bool IsKeyShadowable(string? key) =>
            key != null
            && _parentPaths.Any(x => RegexUtils.IsMatch(key, x))
            && _originConfigurationProvider.TryGet(key, out _);

        public override bool TryGet(string key, out string value)
        {
            if (key.EndsWith(":0", StringComparison.Ordinal)
                && IsKeyShadowable(key[..^2])) {
                value = string.Empty;
                return true;
            }

            value = string.Empty;
            return false;
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            if (IsKeyShadowable(parentPath)) {
                return new List<string>(earlierKeys) {
                    "0"
                };
            }

            return earlierKeys;
        }

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => _parentPaths.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)this).GetEnumerator();
    }
}
