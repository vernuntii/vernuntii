namespace Vernuntii.MessagesProviders;

/// <summary>
/// The interfaces provides messages.
/// </summary>
public interface IMessagesProvider
{
    /// <summary>
    /// Gets the messages.
    /// </summary>
    public IEnumerable<IMessage> GetMessages();
}
