namespace Vernuntii.MessagesProviders
{
    /// <summary>
    /// A flyweight for the commit message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The commit message.
        /// </summary>
        string? Content { get; }
    }
}
