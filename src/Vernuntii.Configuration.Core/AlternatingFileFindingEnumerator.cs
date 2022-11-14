using System.Collections;

namespace Vernuntii.Configuration
{
    internal class AlternatingFileFindingEnumerator : IFileFindingEnumerator
    {
        private static bool FalsyMoveNext() => false;

        public DirectoryInfo? Current => CurrentEnumerator().Current;

        private readonly List<IFileFindingEnumerator> _enumerators;
        private IFileFindingEnumerator? _currentEnumerator;
        private int _position = -1;
        private Func<bool> _moveNextFunc;

        object? IEnumerator.Current => Current;

        public AlternatingFileFindingEnumerator(IEnumerable<IFileFindingEnumerator> enumerators)
        {
            _enumerators = new List<IFileFindingEnumerator>(enumerators);

            if (_enumerators.Count == 0) {
                _moveNextFunc = FalsyMoveNext;
            } else {
                _moveNextFunc = MoveNextCore;
            }
        }

        private IFileFindingEnumerator CurrentEnumerator() =>
            _currentEnumerator ?? throw new InvalidOperationException();

        private void DisableMoveNext()
        {
            _currentEnumerator = null;
            _moveNextFunc = FalsyMoveNext;
        }

        private void StopEnumerating()
        {
            foreach (var enumerator in _enumerators) {
                enumerator.Dispose();
            }

            _enumerators.Clear();
        }

        private bool MoveNextCore()
        {
            while (_enumerators.Count > 0) {
                if (++_position >= _enumerators.Count) {
                    _position = 0;
                }

                var currentIndex = _position;
                var currentEnumerator = _enumerators[currentIndex];

                if (!currentEnumerator.MoveNext()) {
                    _enumerators.RemoveAt(currentIndex);
                    currentEnumerator.Dispose();
                    _position--;
                    continue;
                }

                _currentEnumerator = currentEnumerator;

                if (currentEnumerator.Current is not null) {
                    StopEnumerating();
                }

                return true;
            }

            DisableMoveNext();
            return false;
        }

        public bool MoveNext() => _moveNextFunc();

        public void Reset() => throw new NotImplementedException();

        public string GetCurrentFilePath() => CurrentEnumerator().GetCurrentFilePath();

        public string GetUpwardFilePath()
        {
            if (_currentEnumerator?.Current is not null) {
                return GetUpwardFilePath();
            } else if (MoveNext()) {
                do {
                    if (CurrentEnumerator().Current is not null) {
                        return GetCurrentFilePath();
                    }
                } while (MoveNext());
            }

            return GetCurrentFilePath();
        }

        public void Dispose()
        {
            StopEnumerating();
            DisableMoveNext();
        }
    }
}
