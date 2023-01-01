namespace Vernuntii.VersionPersistence.IO;

internal interface IVersionCacheFileFactory
{
    IVersionCacheFile Open(string filePath);
}
