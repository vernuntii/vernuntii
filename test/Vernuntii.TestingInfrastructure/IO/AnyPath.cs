using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.IO
{
    public class AnyPath : IEquatable<AnyPath>, IAnyPath
    {
        public string PathString { get; }

        internal AnyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            }

            PathString = new FileInfo(path).FullName;
        }

        public bool Equals(IAnyPath? other) =>
            string.Equals(PathString, other?.PathString, StringComparison.Ordinal);

        public bool Equals(AnyPath? other) =>
            Equals((IAnyPath?)other);

        public override bool Equals(object? obj) =>
            Equals(obj as IAnyPath);

        public override int GetHashCode() =>
            PathString.GetHashCode(StringComparison.Ordinal);

        [return: NotNullIfNotNull("path")]
        public static implicit operator string?(AnyPath? path) =>
            path?.PathString;

        [return: NotNullIfNotNull("path")]
        public static implicit operator AnyPath?(string? path) =>
            path == null ? null : new AnyPath(path);

        public static AnyPath operator /(AnyPath path, string appendix) =>
            new(Path.Combine(path ?? "", appendix));

        public static FilePath operator +(AnyPath path, string fileName) =>
            new(Path.Combine(path ?? "", fileName));

        public override string ToString() => PathString;
    }
}
