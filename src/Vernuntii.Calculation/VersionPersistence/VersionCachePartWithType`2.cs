namespace Vernuntii.VersionPersistence;

public class VersionCachePartWithType<T> : VersionCachePartWithType
{
    internal VersionCachePartWithType(string name)
        : base(name, typeof(T)) { }
}
