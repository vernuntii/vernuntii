using System.Runtime.InteropServices;

namespace Vernuntii.Coroutines;

[StructLayout(LayoutKind.Auto)]
internal struct ValueTaskAccessor
{
    internal object? _obj;
    internal short _token;
    internal bool _continueOnCapturedContext;
}
