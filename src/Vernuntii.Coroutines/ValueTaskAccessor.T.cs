using System.Runtime.InteropServices;

namespace Vernuntii.Coroutines;

[StructLayout(LayoutKind.Auto)]
internal struct ValueTaskAccessor<TResult>
{
    internal object? _obj;
    [AllowNull] internal readonly TResult _result;
    internal short _token;
    internal bool _continueOnCapturedContext;
}
