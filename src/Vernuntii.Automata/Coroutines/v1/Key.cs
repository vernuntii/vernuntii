using System.Runtime.InteropServices;
using System.Text;

namespace Vernuntii.Coroutines.v1;

internal enum KeyFlags : byte
{
    Service = 1
}

// 0.-3. (4 bytes) -> hash
// 4. (1 bytes) -> schema version
// 5. (1 bytes) -> flags
// 6.-23. (18 bytes) -> scope
// 24.-31. (8 bytes) -> argument
// 6.-31. (8 bytes) -> service
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Key : IKey
{
    internal const byte CurrentSchemaVersion = 1;

    private const int ArgumentTypeLength = 32;
    private const int ScopeLength = 16;
    private const int ArgumentLength = 10;

    [FieldOffset(0)]
    private readonly int _hash;

    [field: FieldOffset(4)]
    public readonly byte SchemaVersion { get; } = CurrentSchemaVersion;

    [field: FieldOffset(5)]
    private readonly byte _flags;

    [FieldOffset(6)]
    private unsafe fixed byte _scope[ScopeLength];

    [FieldOffset(24)]
    private unsafe fixed byte _argument[ArgumentLength];

    [FieldOffset(6)]
    private unsafe fixed byte _service[ScopeLength];

    public unsafe Key(Span<byte> scope, Span<byte> argument)
    {
        if (scope.Length > ScopeLength) {
            throw new ArgumentOutOfRangeException(nameof(scope), $"Scope must be {ScopeLength} bytes or lesser");
        }

        if (argument.Length > ArgumentLength) {
            throw new ArgumentOutOfRangeException(nameof(argument), $"Argument must be {ArgumentLength} bytes or lesser");
        }

        var hashCode = new HashCode();
        hashCode.Add(SchemaVersion);
        hashCode.Add(_flags);

        fixed (byte* pointer = _scope) {
            hashCode.AddBytes(scope);
            var span = new Span<byte>(pointer, ScopeLength);
            scope.CopyTo(span);
        }

        fixed (byte* pointer = _argument) {
            hashCode.AddBytes(argument);
            var span = new Span<byte>(pointer, ArgumentLength);
            argument.CopyTo(span);
        }

        _hash = hashCode.ToHashCode();
    }

    public unsafe Key(byte[] scope, byte[] argument)
        : this(scope.AsSpan(), argument.AsSpan())
    {
    }

    public unsafe Key(Span<byte> service) { 
        
    }

    internal unsafe bool SequenceEqual(in Key other)
    {
        fixed (Key* thisPointer = &this) {
            fixed (Key* otherPointer = &other) {
                var thisSpan = new Span<byte>(thisPointer, ArgumentTypeLength);
                var otherSpan = new Span<byte>(otherPointer, ArgumentTypeLength);
                return thisSpan.SequenceEqual(otherSpan);
            }
        }
    }

    public override unsafe string ToString()
    {
        fixed (byte* scope = _scope) {
            fixed (byte* argument = _argument) {
                return $$"""CoroutineKey{v{{SchemaVersion}}/{{Encoding.ASCII.GetString(scope, ScopeLength)}}/{{Encoding.ASCII.GetString(argument, ArgumentLength)}}""";
            }
        }
    }

    public override int GetHashCode() => _hash;
}
