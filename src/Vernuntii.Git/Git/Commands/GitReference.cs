using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Git.Commands
{
    internal record GitReference : IGitReference
    {
        public static bool TryParse(string? value, [NotNullWhen(true)] out GitReference? reference)
        {
            if (string.IsNullOrEmpty(value)) {
                goto exit;
            }

            var firstSpace = value.IndexOf(' ', StringComparison.Ordinal);

            if (firstSpace == -1) {
                goto exit;
            }

            var objectName = value[..firstSpace];
            var referenceName = value[(firstSpace + 1)..];
            reference = new GitReference(objectName, referenceName);
            return true;

            exit:
            reference = null;
            return false;
        }

        public string ObjectName { get; }
        public string ReferenceName { get; }

        public GitReference(string objectName, string referenceName)
        {
            if (string.IsNullOrWhiteSpace(objectName)) {
                throw new ArgumentException($"'{nameof(objectName)}' cannot be null or whitespace.", nameof(objectName));
            }

            if (string.IsNullOrWhiteSpace(referenceName)) {
                throw new ArgumentException($"'{nameof(referenceName)}' cannot be null or whitespace.", nameof(referenceName));
            }

            ObjectName = objectName;
            ReferenceName = referenceName;
        }

        protected GitReference(GitReference original)
        {
            ObjectName = original.ObjectName;
            ReferenceName = original.ReferenceName;
        }
    }
}
