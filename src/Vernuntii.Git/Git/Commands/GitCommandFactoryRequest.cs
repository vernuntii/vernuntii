using Vernuntii.Git;
using Vernuntii.Git.Commands;

namespace Vernuntii.Plugins;

/// <summary>
/// Requests an instance of <see cref="IGitCommandFactory"/>.
/// </summary>
public sealed class GitCommandFactoryRequest
{
    public IGitDirectoryResolver? GitDirectoryResolver { get; set; }

    /// <summary>
    /// The git command factory that will be registered to the service collection.
    /// If <see langword="null"/>, then <see cref="Git.Commands.GitCommandFactory"/> is used.
    /// </summary>
    public IGitCommandFactory? GitCommandFactory { get; set; }
}
