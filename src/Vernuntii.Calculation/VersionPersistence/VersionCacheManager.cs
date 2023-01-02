using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MessagePack;
using Microsoft.Extensions.Logging;
using Vernuntii.SemVer;
using Vernuntii.Serialization;
using Vernuntii.VersionPersistence.IO;
using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence;

/// <summary>
/// Manages the version cache.
/// </summary>
public class VersionCacheManager : IVersionCacheManager
{

    /// <inheritdoc/>
    public string CacheId { get; }

    private readonly IVersionCacheDirectory _cacheDirectory;
    private readonly TimeSpan? _creationRetentionTime;
    private readonly bool _useLastAccessRetentionTime;
    private readonly TimeSpan? _lastAccessRetentionTime;
    private readonly IVersionCacheEvaluator _versionCacheEvaluator;
    private readonly IVersionCacheFileFactory _cacheFileFactory;
    private readonly bool _emptyCaches;
    private readonly ILogger<VersionCacheManager> _logger;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="cacheDirectory"></param>
    /// <param name="cacheOptions"></param>
    /// <param name="cacheFileFactory"></param>
    /// <param name="versionCacheEvaluator"></param>
    /// <param name="logger"></param>
    internal VersionCacheManager(
        IVersionCacheDirectory cacheDirectory,
        IVersionCacheEvaluator versionCacheEvaluator,
        IVersionCacheOptions cacheOptions,
        IVersionCacheFileFactory cacheFileFactory,
        ILogger<VersionCacheManager> logger)
    {
        TimeSpan? creationRetentionTime = cacheOptions.CacheCreationRetentionTime;

        var cacheId = cacheOptions.CacheId?.Trim();
        var lastAccessRetentionTime = cacheOptions.LastAccessRetentionTime;
        var useInternalCache = false;

        if (string.Equals(cacheId, cacheOptions.InternalCacheId, StringComparison.OrdinalIgnoreCase)) {
            logger.LogWarning(
                "The versionCache id is equals to \"{InternalCacheId}\" and is reserved internally. The following will happen:" +
                " 1. The creation retention time won't be used if specified." +
                " 2. Last access retention time of {InternalCacheLastAccessRetentionTotalSeconds}s" +
                " is used if not specified. If you use this versionCache id consciously then you can safely ignore this warning.",
                cacheOptions.InternalCacheId,
                cacheOptions.InternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture));

            useInternalCache = true;
        }

        if (string.IsNullOrEmpty(cacheId)) {
            cacheId = cacheOptions.InternalCacheId;
            useInternalCache = true;
        }

        if (useInternalCache) {
            creationRetentionTime = null;
            lastAccessRetentionTime ??= cacheOptions.InternalCacheLastAccessRetentionTime;
        }

        var useLastAccessRetentionTime = lastAccessRetentionTime != null;

        _cacheDirectory = cacheDirectory;
        CacheId = cacheId;
        _creationRetentionTime = creationRetentionTime;
        _useLastAccessRetentionTime = useLastAccessRetentionTime;
        _lastAccessRetentionTime = lastAccessRetentionTime;
        _versionCacheEvaluator = versionCacheEvaluator;
        _cacheFileFactory = cacheFileFactory;
        _emptyCaches = cacheOptions.EmptyCaches;
        _logger = logger;
    }

    private IVersionCacheFile OpenCacheFile()
    {
        _cacheDirectory.CreateCacheDirectoryIfNotExisting();
        var cacheFileName = CacheId + ".data";
        var cacheFilePath = Path.Combine(_cacheDirectory.CacheDirectoryPath, cacheFileName);
        return _cacheFileFactory.Open(cacheFilePath);
    }

    private RecacheIndicator GetRecacheIndicator(ISemanticVersion? comparableVersion)
    {
        IVersionCache? versionCache = null;
        IManagedValueWriter<IVersionCache>? versionCacheWriter = null;
        bool isRecacheRequired;
        string? recacheReason;

        try {
            var cacheFile = OpenCacheFile();
            versionCacheWriter = cacheFile;

            if (!cacheFile.CanRead) {
                isRecacheRequired = true;
                recacheReason = "Cache is empty";
            } else {
                versionCache = cacheFile.ReadCache();

                isRecacheRequired = _versionCacheEvaluator.IsRecacheRequired(
                    versionCache,
                    _useLastAccessRetentionTime,
                    _lastAccessRetentionTime,
                    comparableVersion,
                    out recacheReason);
            }
        } catch (VersionCacheSerializationException error) when (versionCacheWriter is not null) {
            isRecacheRequired = true;
            recacheReason = "Cache is damaged";
            _logger.LogError(error, "{RecacheReason} and a recache is inevitable", recacheReason);
        } catch {
            versionCacheWriter?.Dispose();
            throw;
        }

        return new RecacheIndicator(
                versionCache,
                versionCacheWriter,
                isRecacheRequired,
                recacheReason);
    }

    /// <inheritdoc/>
    public bool IsRecacheRequired([NotNullWhen(false)] out IVersionCache? versionCache)
    {
        using var recacheIndicator = GetRecacheIndicator(comparableVersion: null);
        var isRecacheRequired = recacheIndicator.IsRecacheRequired(out var loadedVersionCache, out var versionCacheWriter);

        if (!isRecacheRequired
            && loadedVersionCache != null
            && _useLastAccessRetentionTime) {
            var newLastAccessTime = DateTime.UtcNow;
            versionCache = new VersionCache(loadedVersionCache) { LastAccessTime = newLastAccessTime };

            _logger.LogInformation(
                "Updated last access time of version cache to {LastAccessTime} (UTC)",
                newLastAccessTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture));

            versionCacheWriter.Overwrite(versionCache);
        } else {
            versionCache = loadedVersionCache;
        }

        return isRecacheRequired;
    }

    /// <inheritdoc/>
    public IVersionCache RecacheCache(ISemanticVersion newVersion, IImmutableVersionCacheDataTuples versionCacheDataTuples)
    {
        using var cacheFile = OpenCacheFile();

        var versionCache = new VersionCache(newVersion, versionCacheDataTuples, skipDataLookup: true) {
            ExpirationTime = ExpirationTime.FromCreationRetentionTime(_creationRetentionTime),
            LastAccessTime = _useLastAccessRetentionTime ? DateTime.UtcNow : null,
        };

        cacheFile.Overwrite(versionCache);
        return versionCache;
    }

    private class RecacheIndicator : IDisposable
    {
        private readonly IVersionCache? _versionCache;
        public string? RecacheReason { get; }

        private readonly bool _isRecacheRequired;

        private readonly IManagedValueWriter<IVersionCache> _versionCacheWriter;

        public RecacheIndicator(
            IVersionCache? versionCache,
            IManagedValueWriter<IVersionCache> disposableVersionCacheWriter,
            bool isRecacheRequired,
            string? recacheReason)
        {
            _versionCache = versionCache;
            _versionCacheWriter = disposableVersionCacheWriter ?? throw new ArgumentNullException(nameof(disposableVersionCacheWriter));
            _isRecacheRequired = isRecacheRequired;
            RecacheReason = recacheReason;
        }

        [MemberNotNullWhen(true, nameof(RecacheReason))]
        public bool IsRecacheRequired(
            [NotNullWhen(false)] out IVersionCache? versionCache,
            out IValueWriter<IVersionCache> versionCacheWriter)
        {
            versionCache = _versionCache;
            versionCacheWriter = _versionCacheWriter;
            return _isRecacheRequired;
        }

        public void Dispose() => _versionCacheWriter.Dispose();
    }
}
