using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.IO
{
    public class FilePath : IEquatable<FilePath>, IEquatable<IAnyPath>
    {
        public string FileName { get; }
        public string PathString { get; }

        internal FilePath(string path)
        {
            PathString = path ?? throw new ArgumentNullException(nameof(path));
            FileName = Path.GetFileName(path);
        }

        public bool Equals(IAnyPath? other) =>
            string.Equals(PathString, other?.PathString, StringComparison.Ordinal);

        public bool Equals(FilePath? other) =>
            Equals((IAnyPath?)other);

        public override bool Equals(object? obj) =>
            Equals(obj as IAnyPath);

        public override int GetHashCode() =>
            PathString.GetHashCode(StringComparison.Ordinal);

        [return: NotNullIfNotNull("path")]
        public static implicit operator string?(FilePath? path) =>
            path?.PathString;

        [return: NotNullIfNotNull("path")]
        public static implicit operator FilePath?(string? path) =>
            path == null ? null : new FilePath(path);
    }
}
