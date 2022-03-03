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
using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace GitTools.Testing;

/// <summary>
///     Creates a remote repository then clones it
///     Remote = Repository
///     Local  = LocalRepositoryFixture
/// </summary>
public class RemoteRepositoryFixture : RepositoryFixtureBase
{
    public RemoteRepositoryFixture(Func<string, IRepositoryBridge> builder)
        : base(builder) => CreateLocalRepository();

    public RemoteRepositoryFixture() : this(CreateNewRepository)
    {
    }

    /// <summary>
    ///     Fixture pointing at the local repository
    /// </summary>
    public LocalRepositoryFixture LocalRepositoryFixture { get; private set; }

    private static IRepositoryBridge CreateNewRepository(string path)
    {
        Init(path);
        Console.WriteLine("Created git repository at '{0}'", path);

        var repo = new RepositoryBridge(new Repository(path));
        repo.MakeCommits(5);
        return repo;
    }

    [MemberNotNull(nameof(LocalRepositoryFixture))]
    private void CreateLocalRepository() => LocalRepositoryFixture = CloneRepository();

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) {
            LocalRepositoryFixture.Dispose();
        }

        base.Dispose(disposing);
    }
}
