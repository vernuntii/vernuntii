using Vernuntii.Text;

namespace Vernuntii.Git.Command
{
    internal class TestingGitCommand : GitCommand
    {
        public TestingGitCommand(string workingDirectory)
            : base(workingDirectory)
        {
        }

        public int Init() => ExecuteCommand("init");

        public void SetConfig(NullableQuote name, NullableQuote value)
        {
            var args = CultureStringBuilder.Invariant();
            args.Append("config");
            args.Append($" {name}");
            args.Append($" {value}");
            ExecuteCommandThenSucceed(args);
        }

        /// <summary>
        /// Creates a commit.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowEmpty"></param>
        /// <param name="allowEmptyMessage"></param>
        public void Commit(NullableQuote message = default, bool allowEmpty = false, bool allowEmptyMessage = false)
        {
            var args = CultureStringBuilder.Invariant();
            args.Append("commit");

            if (message.Content != null) {
                args.Append($" -m {message}");
            }

            if (allowEmpty) {
                args.Append($" --allow-empty");
            }

            if (allowEmptyMessage) {
                args.Append($" --allow-empty-message");
            }

            ExecuteCommandThenSucceed(args);
        }

        public void TagLightweight(NullableQuote tagName, NullableQuote commit = default)
        {
            var args = CultureStringBuilder.Invariant();
            args.Append("tag");
            args.Append($" {tagName}");

            if (commit.Content != null) {
                args.Append($" {commit}");
            }

            ExecuteCommandThenSucceed(args);
        }

        /// <summary>
        /// Clones a repository in a new directory.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="depth"></param>
        public void Clone(NullableQuote url, int? depth = null)
        {
            var args = CultureStringBuilder.Invariant();
            args.Append("clone");

            if (depth != null) {
                args.Append($" --depth {depth}");
            }

            args.Append($" {url} {(Quote)WorkingTreeDirectory}");
            ExecuteCommandThenSucceed(args);
        }

        public void Checkout(Quote branchName)
        {
            var args = CultureStringBuilder.Invariant();
            args.Append($"checkout {branchName}");
            ExecuteCommandThenSucceed(args);
        }
    }
}
