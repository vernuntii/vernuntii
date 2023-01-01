using Vernuntii.Plugins;
using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence.Presentation;

public static class VersionCacheManagerContextExtensions
{
    /// <summary>
    /// Adds a serializer to <see cref="VersionCacheManagerContext.Serializers"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="identifier"></param>
    /// <param name="tuple"></param>
    public static void AddSerializer(this VersionCacheManagerContext options, object identifier, VersionCacheFormatterTuple tuple) =>
        options.Serializers.Add(identifier, tuple);

    /// <summary>
    /// Adds a serializer to <see cref="VersionCacheManagerContext.Serializers"/>.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="identifier"></param>
    /// <param name="formatter"></param>
    /// <param name="deformatter"></param>
    public static void AddSerializer(
        this VersionCacheManagerContext options,
        object identifier,
        IVersionCacheFormatter formatter,
        IVersionCacheDeformatter deformatter) =>
        options.Serializers.Add(identifier, new VersionCacheFormatterTuple(formatter, deformatter));

    /// <summary>
    /// Imports git requirements which includes
    /// a serializer (<see cref="GitVersionCacheFormatter"/> and <see cref="GitVersionCacheDeformatter"/> identifiable via <see cref="GitPlugin.VersionCacheManagerSerializerIdentifier"/>)
    /// </summary>
    /// <param name="options"></param>
    public static void ImportGitRequirements(this VersionCacheManagerContext options) =>
        options.AddSerializer(GitPlugin.VersionCacheManagerSerializerIdentifier, new GitVersionCacheFormatter(), new GitVersionCacheDeformatter());
}
