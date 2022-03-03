namespace Vernuntii.Git
{
    internal record Commit : ICommit
    {
        public string Sha { get; }
        public string Subject { get; }

        public Commit(string sha, string subject)
        {
            Sha = sha ?? throw new ArgumentNullException(nameof(sha));
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        }
    }
}
