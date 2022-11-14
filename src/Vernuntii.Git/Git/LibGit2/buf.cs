using System.Runtime.InteropServices;

namespace Vernuntii.Git.LibGit2
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe class git_buf
    {
        public byte* ptr;
        public UIntPtr asize;
        public UIntPtr size;
    }
}
