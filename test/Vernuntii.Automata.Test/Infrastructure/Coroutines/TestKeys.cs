using Vernuntii.Coroutines;

namespace Vernuntii.Infrastructure.Coroutines;

internal class TestKeys
{
    private static Key? s_key1;

    public static Key Key1 {
        get {
            if (s_key1.HasValue) {
                return s_key1.Value;
            }

            Span<byte> bytes = stackalloc byte[Key.ServiceLength];
            bytes.Fill(byte.MaxValue);
            var key = new Key(service: bytes, inheritable: true);
            s_key1 = key;
            return key;
        }
    }
}
