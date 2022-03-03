using System.Collections;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Enumerables identifiers.
    /// </summary>
    internal sealed class IdentifierEnumerable : IEnumerable<string>
    {
        /// <summary>
        /// Returns an empty instance of <see cref="IdentifierEnumerable"/>.
        /// </summary>
        public readonly static IdentifierEnumerable Empty = new IdentifierEnumerable(Array.Empty<string>());

        private readonly string[] _identifiers;

        internal IdentifierEnumerable(string[] identifiers) =>
            _identifiers = identifiers ?? throw new ArgumentNullException(nameof(identifiers));

        /// <inheritdoc/>
        public IEnumerator<string> GetEnumerator() => new IdentifierEnumerator(_identifiers);

        IEnumerator IEnumerable.GetEnumerator() => _identifiers.GetEnumerator();

        private sealed class IdentifierEnumerator : IEnumerator<string>
        {
            public string Current => _preReleaseIdentifiers[_currentIndex];

            private readonly string[] _preReleaseIdentifiers;
            private int _currentIndex = -1;

            object IEnumerator.Current => Current;

            public IdentifierEnumerator(string[] preReleaseIdentifiers) =>
                _preReleaseIdentifiers = preReleaseIdentifiers;

            public bool MoveNext()
            {
                while (_currentIndex + 1 < _preReleaseIdentifiers.Length) {
                    _currentIndex++;

                    if (string.IsNullOrEmpty(Current)) {
                        continue;
                    }

                    return true;
                }

                return false;
            }

            public void Reset() => _currentIndex = 0;

            public void Dispose()
            {
            }
        }
    }
}
