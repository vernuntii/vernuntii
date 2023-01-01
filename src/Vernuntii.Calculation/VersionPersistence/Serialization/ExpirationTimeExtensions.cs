namespace Vernuntii.VersionPersistence.Serialization;

internal static class ExpirationTimeExtensions
{
    public static ExpirationTime ToExpirationTime(this DateTime? expirationTime) =>
        new ExpirationTime(expirationTime);
}
