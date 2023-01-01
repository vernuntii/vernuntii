using Vernuntii.Serialization;

namespace Vernuntii.VersionPersistence.IO;

internal interface IVersionCacheFile : IManagedValueWriter<IVersionCache>
{
    bool CanRead { get; }

    IVersionCache ReadCache();
}
