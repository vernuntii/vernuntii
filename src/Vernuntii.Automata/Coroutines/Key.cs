using System.Runtime.InteropServices;
using System.Text;

namespace Vernuntii.Coroutines;

internal enum KeyFlags : byte
{
    None = 0,
    ContextService = 1,
    Service = 2,
    Inheritable = 4,
}

// 0.-3. (4 bytes) -> hash
// 4. (1 bytes) -> schema version
// 5. (1 bytes) -> flags
// 6.-23. (6 bytes) -> scope
// 24.-31. (4 bytes) -> argument
// 6.-31. (10 bytes) -> service
[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct Key : IKey
{
    internal const byte CurrentSchemaVersion = 1;

    private const int KeyLength = 20;
    private const int ScopeLength = 14;
    private const int ArgumentLength = 2;
    private const int ServiceLength = 16;

    [FieldOffset(0)]
    private readonly int _hash;

    [field: FieldOffset(4)]
    public readonly byte SchemaVersion { get; } = CurrentSchemaVersion;

    [field: FieldOffset(5)]
    internal readonly byte Flags;

    [FieldOffset(6)]
    private unsafe fixed byte _scope[ScopeLength];

    [FieldOffset(18)]
    private ushort _argument;

    [FieldOffset(6)]
    private unsafe fixed byte _service[ServiceLength];

    public unsafe Key(Span<byte> scope, ushort argument)
    {
        if (scope.Length > ScopeLength) {
            throw new ArgumentOutOfRangeException(nameof(scope), $"Scope must be {ScopeLength} bytes or lesser");
        }

        var hashCode = new HashCode();
        hashCode.Add(SchemaVersion);
        hashCode.Add(Flags);

        fixed (byte* scopePointer = _scope) {
            var scopeSpan = new Span<byte>(scopePointer, ScopeLength);
            scope.CopyTo(scopeSpan);
            hashCode.AddBytes(scopeSpan);
        }

        _argument = argument;
        hashCode.Add(argument);

        _hash = hashCode.ToHashCode();
    }

    public unsafe Key(byte[] scope, ushort argument)
        : this(scope.AsSpan(), argument)
    {
    }

    internal unsafe Key(Span<byte> service, bool isContextService = false, bool inheritable = false)
    {
        if (service.Length > ServiceLength) {
            throw new ArgumentOutOfRangeException(nameof(service), $"Scope must be {ServiceLength} bytes or lesser");
        }

        var hashCode = new HashCode();
        hashCode.Add(SchemaVersion);

        Flags = (byte)((isContextService ? KeyFlags.ContextService : KeyFlags.Service) | (inheritable ? KeyFlags.Inheritable : KeyFlags.None));
        hashCode.Add(Flags);

        fixed (byte* servicePointer = _service) {
            var serviceSpan = new Span<byte>(servicePointer, ServiceLength);
            service.CopyTo(serviceSpan);
            hashCode.AddBytes(serviceSpan);
        }

        _hash = hashCode.ToHashCode();
    }

    public unsafe Key(Span<byte> service, bool inheritable) : this(service, isContextService: false, inheritable: inheritable)
    {
    }

    internal unsafe bool SequenceEqual(in Key other)
    {
        fixed (Key* thisPointer = &this) {
            fixed (Key* otherPointer = &other) {
                var thisSpan = new Span<byte>(thisPointer, KeyLength);
                var otherSpan = new Span<byte>(otherPointer, KeyLength);
                return thisSpan.SequenceEqual(otherSpan);
            }
        }
    }

    public override unsafe string ToString()
    {
        var isService = (Flags & (byte)KeyFlags.Service) == (byte)KeyFlags.Service ||
            (Flags & (byte)KeyFlags.ContextService) == (byte)KeyFlags.ContextService;

        var sb = new StringBuilder();
        sb.Append("Key{v");
        sb.Append(SchemaVersion);
        sb.Append('/');

        if (isService) {
            fixed (byte* service = _service) {
                sb.Append(Encoding.ASCII.GetString(service, ServiceLength));
            }
        } else {
            fixed (byte* scope = _scope) {
                sb.Append(Encoding.ASCII.GetString(scope, ScopeLength));
                sb.Append('/');
                sb.Append(_argument);
            }
        }

        sb.Append('}');
        return sb.ToString();
    }

    public override int GetHashCode() => _hash;
}
