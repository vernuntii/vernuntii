using System.Globalization;
using System.Text;

namespace Vernuntii.Git.Command
{
    internal class TestingGitCommand : GitCommand
    {
        public TestingGitCommand(string workingDirectory)
            : base(workingDirectory)
        {
        }

        public int Init() => ExecuteCommand("init");

        public void SetConfig(QuotedString name, QuotedString value)
        {
            var args = new StringBuilder();
            args.Append(" config");
            args.Append(CultureInfo.InvariantCulture, $" {name.Value}");
            args.Append(CultureInfo.InvariantCulture, $" {value.Value}");
            ExecuteCommandThenSucceed(args.ToString());
        }

        /// <summary>
        /// Creates a commit.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="allowEmpty"></param>
        /// <param name="allowEmptyMessage"></param>
        public void Commit(QuotedString message = default, bool allowEmpty = false, bool allowEmptyMessage = false)
        {
            var args = new StringBuilder();
            args.Append(" commit");

            if (message.Value != null) {
                args.Append(CultureInfo.InvariantCulture, $" -m {message.Value}");
            }

            if (allowEmpty) {
                args.Append(CultureInfo.InvariantCulture, $" --allow-empty");
            }

            if (allowEmptyMessage) {
                args.Append(CultureInfo.InvariantCulture, $" --allow-empty-message");
            }

            ExecuteCommandThenSucceed(args.ToString());
        }

        public void TagLightweight(QuotedString tagName, QuotedString commit = default)
        {
            var args = new StringBuilder();
            args.Append(" tag");
            args.Append(CultureInfo.InvariantCulture, $" {tagName.Value}");

            if (commit.Value != null) {
                args.Append(CultureInfo.InvariantCulture, $" {commit.Value}");
            }

            ExecuteCommandThenSucceed(args.ToString());
        }

        public readonly struct QuotedString
        {
            public static QuotedString DoubleQuoted(string message) =>
                new QuotedString($"\"{message}\"");

            public static QuotedString SingleQuoted(string message) =>
                new QuotedString($"'{message}'");

            public string? Value { get; }

            public QuotedString(string message) =>
                Value = message;

            public static implicit operator QuotedString(string? message) =>
                message == null ? default : DoubleQuoted(message);
        }
    }
}
