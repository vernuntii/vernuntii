namespace Vernuntii.Git.Commands;

/// <summary>
/// Requests an instance of <see cref="IGitCommandFactory"/>.
/// </summary>
public sealed class GitCommandFactoryRequest
{
    /// <summary>
    /// The git-specific directory resolver.
    /// </summary>
    public IGitDirectoryResolver? GitDirectoryResolver { get; set; }

    /// <summary>
    /// The git command factory that will be registered to the service collection.
    /// If <see langword="null"/>, then <see cref="Commands.GitCommandFactory"/> is used.
    /// </summary>
    public IGitCommandFactory? GitCommandFactory { get; set; }
}
