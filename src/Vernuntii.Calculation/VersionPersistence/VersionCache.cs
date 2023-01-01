using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence;

internal record VersionCache : IVersionCache
{
    /// <inheritdoc/>
    public ISemanticVersion Version { get; }

    /// <inheritdoc/>
    public ExpirationTime ExpirationTime { get; init; }

    /// <inheritdoc/>
    public DateTime? LastAccessTime { get; init; }

    private readonly IImmutableVersionCacheDataTuples _dataTuples;

    internal VersionCache(ISemanticVersion version, IImmutableVersionCacheDataTuples dataTuples, bool skipDataLookup)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));
        _dataTuples = dataTuples ?? throw new ArgumentNullException(nameof(dataTuples));

        if (!skipDataLookup) {
            ExpirationTime = dataTuples.GetDataOrDefault(VersionCacheParts.ExpirationTime);
            LastAccessTime = dataTuples.GetDataOrDefault(VersionCacheParts.LastAccessTime);
        }
    }

    public VersionCache(IImmutableVersionCacheDataTuples dataTuples)
    {
        _dataTuples = dataTuples ?? throw new ArgumentNullException(nameof(dataTuples));

        if (!dataTuples.TryGetData(VersionCacheParts.Version, out var version)) {
            throw new ArgumentException($"{nameof(VersionCacheParts.Version)} is not retrievable from data tuples", nameof(dataTuples));
        }

        Version = version ?? throw new ArgumentNullException(nameof(version));
        ExpirationTime = dataTuples.GetDataOrDefault(VersionCacheParts.ExpirationTime);
        LastAccessTime = dataTuples.GetDataOrDefault(VersionCacheParts.LastAccessTime);
    }

    public VersionCache(ISemanticVersion version)
    {
        Version = version ?? throw new ArgumentNullException(nameof(version));
        _dataTuples = ImmutableVersionCacheDataTuples.Empty;
    }

    public virtual bool TryGetAdditionalData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType) =>
        _dataTuples.TryGetData(part, out data, out dataType);

    public virtual bool TryGetData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType)
    {
        if (VersionCacheParts.Major == part) {
            data = Version.Major;
            dataType = VersionCacheParts.Major.Type;
        } else if (VersionCacheParts.Minor == part) {
            data = Version.Minor;
            dataType = VersionCacheParts.Minor.Type;
        } else if (VersionCacheParts.Patch == part) {
            data = Version.Patch;
            dataType = VersionCacheParts.Patch.Type;
        } else if (VersionCacheParts.VersionCore == part) {
            data = Version.Format(SemanticVersionFormat.VersionCore);
            dataType = VersionCacheParts.VersionCore.Type;
        } else if (VersionCacheParts.PreRelease == part) {
            data = Version.PreRelease;
            dataType = VersionCacheParts.PreRelease.Type;
        } else if (VersionCacheParts.HyphenPreRelease == part) {
            data = Version.Format(SemanticVersionFormat.PreRelease);
            dataType = VersionCacheParts.HyphenPreRelease.Type;
        } else if (VersionCacheParts.Build == part) {
            data = Version.Build;
            dataType = VersionCacheParts.Build.Type;
        } else if (VersionCacheParts.PlusBuild == part) {
            data = Version.Format(SemanticVersionFormat.Build);
            dataType = VersionCacheParts.PlusBuild.Type;
        } else if (VersionCacheParts.SemanticVersion == part) {
            data = Version.Format(SemanticVersionFormat.SemanticVersion);
            dataType = VersionCacheParts.SemanticVersion.Type;
        } else if (VersionCacheParts.Version == part) {
            data = Version;
            dataType = VersionCacheParts.Version.Type;
        } else if (TryGetAdditionalData(part, out data, out dataType)) {
            return true;
        } else {
            data = null;
            dataType = null;
            return false;
        }

        return true;
    }

    private class DelegatedVersionCacheDataTuples : IImmutableVersionCacheDataTuples
    {
        private readonly IImmutableVersionCacheDataTuples _versionCacheDataTuples;

        public DelegatedVersionCacheDataTuples(IImmutableVersionCacheDataTuples versionCacheDataTuples) =>
            _versionCacheDataTuples = versionCacheDataTuples ?? throw new ArgumentNullException(nameof(versionCacheDataTuples));

        public bool TryGetData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType) =>
            _versionCacheDataTuples.TryGetData(part, out data, out dataType);
    }
}
