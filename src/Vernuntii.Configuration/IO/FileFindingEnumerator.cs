using System.Collections;

namespace Vernuntii.IO
{
    internal class FileFindingEnumerator : IFileFindingEnumerator
    {
        public DirectoryInfo? Current => _upwardDirectoryEnumerator.Current;
        object? IEnumerator.Current => Current;

        private readonly string _directoryPath;
        private readonly string _fileName;
        private readonly IEnumerator<DirectoryInfo?> _upwardDirectoryEnumerator;

        public FileFindingEnumerator(
            string directoryPath,
            string fileName,
            IEnumerator<DirectoryInfo?> upwardDirectoryEnumerator)
        {
            _directoryPath = directoryPath;
            _fileName = fileName;
            _upwardDirectoryEnumerator = upwardDirectoryEnumerator;
        }

        private string GetCurrentLevelFilePath(DirectoryInfo? current)
        {
            if (current is null) {
                throw new FileNotFoundException($"File not found in {_directoryPath} or above", _fileName);
            }

            return Path.Combine(current.FullName, _fileName);
        }

        public string GetCurrentLevelFilePath() =>
            GetCurrentLevelFilePath(Current);

        public string GetHigherLevelFilePath()
        {
            DirectoryInfo? latestCurrent;

            if (Current is not null) {
                latestCurrent = Current;
            } else if (MoveNext()) {
                do {
                    latestCurrent = _upwardDirectoryEnumerator.Current;
                } while (MoveNext());
            } else {
                latestCurrent = null;
            }

            return GetCurrentLevelFilePath(latestCurrent);
        }

        public bool MoveNext() => _upwardDirectoryEnumerator.MoveNext();

        public void Reset() => _upwardDirectoryEnumerator.Reset();

        public void Dispose() => _upwardDirectoryEnumerator.Dispose();
    }
}
