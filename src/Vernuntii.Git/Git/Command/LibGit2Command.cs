using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.Git.LibGit2;
using Vernuntii.Git.LibGit2.Runtime;

namespace Vernuntii.Git.Commands
{
    internal class LibGit2Command : IDisposable
    {
        private unsafe git_repository* _repositoryPointer;

        // Provide a strongly typed exception when a repository is not
        // found to open.
        private static readonly Dictionary<git_error_code, Func<string, Exception>> exceptionMap = new Dictionary<git_error_code, Func<string, Exception>>
        {
            { git_error_code.GIT_ENOTFOUND, (m) => new RepositoryNotFoundException(m) }
        };

        public unsafe LibGit2Command(string workingTreeDirectory)
        {
            Ensure.NativeSuccess(
                libgit2.git_repository_open(out _repositoryPointer, workingTreeDirectory),
                new Dictionary<git_error_code, Func<string, Exception>>
                {
                    { git_error_code.GIT_ENOTFOUND, (m) => new RepositoryNotFoundException(m) }
                });
        }

        //public bool IsHeadDetached()
        //{
        //    var isHeadDetached = Ensure.NativeCall(

        //    return libgit2.git_repository_head_detached(_repositoryPointer) == 1;
        //}

        public unsafe bool IsShallow() =>
            libgit2.git_repository_head_detached(_repositoryPointer) == 1;

        public unsafe void Dispose()
        {
            if (_repositoryPointer == null) {
                return;
            }

            libgit2.git_repository_free(_repositoryPointer);
            _repositoryPointer = null;
        }
    }
}
