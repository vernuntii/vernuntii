namespace Vernuntii.Git;

/// <summary>
/// Represents the options for <see cref="Repository"/>.
/// </summary>
public sealed class RepositoryOptions
{
    internal static readonly RepositoryOptions s_default = new();

    /// <summary>
    /// If <see langword="true"/> (default), then a <see cref="ShallowRepositoryException"/> is thrown in case of a shallow repository.
    /// </summary>
    public bool AllowShallow { get; set; }
}
