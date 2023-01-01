using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence;

public record VersionCacheFormatterTuple
{
    private readonly IVersionCacheFormatter _formatter;
    private readonly IVersionCacheDeformatter _deformatter;

    public IVersionCacheFormatter Formatter {
        get => _formatter;
        init => _formatter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IVersionCacheDeformatter Deformatter {
        get => _deformatter;
        init => _deformatter = value ?? throw new ArgumentNullException(nameof(value));
    }

    public VersionCacheFormatterTuple(IVersionCacheFormatter formatter, IVersionCacheDeformatter deformatter)
    {
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _deformatter = deformatter ?? throw new ArgumentNullException(nameof(deformatter));
    }
}
