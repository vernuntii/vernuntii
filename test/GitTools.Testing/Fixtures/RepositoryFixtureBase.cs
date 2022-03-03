/* The MIT License (MIT)

Copyright (c) 2021 NServiceBus Ltd, GitTools and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using GitTools.Testing.Internal;
using LibGit2Sharp;

namespace GitTools.Testing;

/// <summary>
///     Fixture abstracting a git repository
/// </summary>
public abstract class RepositoryFixtureBase : IDisposable
{

    protected RepositoryFixtureBase(Func<string, IRepositoryBridge> repoBuilder)
        : this(repoBuilder(PathHelper.GetTempPath()))
    {
    }

    protected RepositoryFixtureBase(IRepositoryBridge repository)
    {
        this.SequenceDiagram = new SequenceDiagram();
        Repository = repository;
        Repository.Config.Set("user.name", "Test");
        Repository.Config.Set("user.email", "test@email.com");
    }

    public IRepositoryBridge Repository { get; }

    public string RepositoryPath => Repository.Info.WorkingDirectory.TrimEnd('\\');

    public SequenceDiagram SequenceDiagram { get; }


    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Repository.Dispose();

        try
        {
            DirectoryHelper.DeleteDirectory(RepositoryPath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to clean up repository path at {0}. Received exception: {1}", RepositoryPath,
                e.Message);
        }

        this.SequenceDiagram.End();
        Console.WriteLine("**Visualisation of test:**");
        Console.WriteLine(string.Empty);
        Console.WriteLine(this.SequenceDiagram.GetDiagram());
    }

    public void Checkout(string branch) => Commands.Checkout(Repository, branch);

    public static void Init(string path) => GitTestExtensions.ExecuteGitCmd($"init {path} -b main");

    public void MakeATaggedCommit(string tag)
    {
        MakeACommit();
        ApplyTag(tag);
    }

    public void ApplyTag(string tag)
    {
        this.SequenceDiagram.ApplyTag(tag, Repository.Head.FriendlyName);
        Repository.ApplyTag(tag);
    }

    public void BranchTo(string branchName, string? @as = null)
    {
        this.SequenceDiagram.BranchTo(branchName, Repository.Head.FriendlyName, @as);
        var branch = Repository.CreateBranch(branchName);
        Commands.Checkout(Repository, branch);
    }

    public void BranchToFromTag(string branchName, string fromTag, string onBranch, string? @as = null)
    {
        this.SequenceDiagram.BranchToFromTag(branchName, fromTag, onBranch, @as);
        var branch = Repository.CreateBranch(branchName);
        Commands.Checkout(Repository, branch);
    }

    public void MakeACommit()
    {
        var to = Repository.Head.FriendlyName;
        this.SequenceDiagram.MakeACommit(to);
        Repository.MakeACommit();
    }

    /// <summary>
    ///     Merges (no-ff) specified branch into the current HEAD of this repository
    /// </summary>
    public void MergeNoFF(string mergeSource)
    {
        this.SequenceDiagram.Merge(mergeSource, Repository.Head.FriendlyName);
        Repository.MergeNoFF(mergeSource, Generate.SignatureNow());
    }

    /// <summary>
    ///     Clones the repository managed by this fixture into another LocalRepositoryFixture
    /// </summary>
    public LocalRepositoryFixture CloneRepository()
    {
        var localPath = PathHelper.GetTempPath();
        LibGit2Sharp.Repository.Clone(RepositoryPath, localPath);
        return new LocalRepositoryFixture(new RepositoryBridge(new Repository(localPath)));
    }
}
