using System.Runtime.InteropServices;

namespace Vernuntii.Coroutines;

// 1. (1 bytes) -> version
// 2. (1 bytes) -> _reserved_
// 3.-4. (2 bytes) -> flags
// 5.-24. byte (20 bytes) -> scope
// 25.-32. byte (8 bytes) -> argument
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ArgumentType : IArgumentType
{
    internal const byte CurrentVersion = 1;

    public readonly byte Version { get; } = CurrentVersion;

    private readonly byte _reserved;

    private readonly ushort _flags;

    private unsafe fixed byte _scope[20];

    private unsafe fixed byte _argument[8];

    public unsafe ArgumentType(Span<byte> scope, Span<byte> argument)
    {
        if (scope.Length > 20) {
            throw new ArgumentOutOfRangeException(nameof(scope), "Scope must be 20 bytes or lesser");
        }

        if (argument.Length > 8) {
            throw new ArgumentOutOfRangeException(nameof(argument), "Argument must be 20 bytes or lesser");
        }

        fixed (byte* pointer = _scope) {
            var span = new Span<byte>(pointer, 20);
            scope.CopyTo(span);
        }

        fixed (byte* pointer = _argument) {
            var span = new Span<byte>(pointer, 8);
            argument.CopyTo(span);
        }
    }

    public unsafe ArgumentType(byte[] scope, byte[] argument)
        : this(scope.AsSpan(), argument.AsSpan())
    {
    }

    internal unsafe bool SequenceEqual(in ArgumentType other)
    {
        fixed (ArgumentType* thisPointer = &this) {
            fixed (ArgumentType* otherPointer = &other) {
                var thisSpan = new Span<byte>(thisPointer, 32);
                var otherSpan = new Span<byte>(otherPointer, 32);
                return thisSpan.SequenceEqual(otherSpan);
            }
        }
    }
}
