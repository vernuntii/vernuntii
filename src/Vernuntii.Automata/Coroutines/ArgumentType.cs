using System.Runtime.InteropServices;

namespace Vernuntii.Coroutines;

// 1. (1 bytes) -> version
// 2.-4. (3 bytes) -> _reserved_
// 5.-24. byte (20 bytes) -> scope
// 25.-32. byte (8 bytes) -> argument
[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct ArgumentType : IArgumentType
{
    internal const byte CurrentVersion = 1;

    [FieldOffset(0)]
    public byte Version = CurrentVersion;

    [FieldOffset(1)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    private byte[] _reserved;

    [FieldOffset(4)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] Scope;

    [FieldOffset(24)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Argument;

    readonly byte IArgumentType.Version => Version;

    public ArgumentType(byte[] scope, byte[] argument)
    {
        _reserved = new byte[3];  // Initialize the reserved space to zero.

        Scope = new byte[20];
        if (scope != null) {
            if (scope.Length != 20) {
                throw new ArgumentException("Scope must be exactly 20 bytes.");
            }
            Array.Copy(scope, Scope, 20);
        }

        Argument = new byte[8];
        if (argument != null) {
            if (argument.Length != 20) {
                throw new ArgumentException("Scope must be exactly 20 bytes.");
            }
            Array.Copy(argument, Argument, 20);
        }
    }
}
