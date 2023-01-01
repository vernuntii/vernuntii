namespace Vernuntii.VersionPersistence.Presentation.Serializers;

internal static class VersionPresentationSerializerHelpers
{
    internal static void GetData(IVersionCache versionCache, IVersionCachePart part, out object? data, out Type dataType)
    {
        if (!versionCache.TryGetData(part, out data, out var uncertainDataType)) {
            throw new VersionPresentationException($"The version cache part {part.Name} was requested to be presented but it does not exist");
        }

        dataType = uncertainDataType;
    }
}
