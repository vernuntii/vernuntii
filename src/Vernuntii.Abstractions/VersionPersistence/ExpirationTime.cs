using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

public readonly struct ExpirationTime
{
    public static readonly ExpirationTime None = new ExpirationTime(time: null);

    public static ExpirationTime FromCreationRetentionTime(TimeSpan? creationRetentionTime) =>
        new ExpirationTime(creationRetentionTime == null ? null : DateTime.UtcNow + creationRetentionTime);

    public DateTime? Time { get; }

    /// <summary>
    /// Determines whether the time represents an expiration time.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Time))]
    public bool IsExpirable =>
        Time != null;

    public ExpirationTime(DateTime? time) =>
        Time = time;

    public bool IsExpired(DateTime time) => IsExpirable
        ? time >= Time
        : false;

    public bool IsExpired() =>
        IsExpired(DateTime.UtcNow);
}
