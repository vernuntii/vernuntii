namespace Vernuntii.VersionPersistence;

internal interface ICacheIdentifierProvider
{
    /// <summary>
    /// The cache identifier to differentiate between different setups.
    /// </summary>
    public string CacheId { get; }
}
