using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public class CoroutineScopeBuilder
{
    private Dictionary<Key, object>? _keyedServices;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Dictionary<Key, object> KeyedServices() => _keyedServices ??= [];

    public void AddKeyedService<TServiceKey, TService>(in TServiceKey serviceKey, TService service)
        where TServiceKey : IKey
        where TService : class
    {
        if (serviceKey.SchemaVersion == 1) {
            if (default(TServiceKey) != null && serviceKey is Key) {
                ref var typedServiceKey = ref Unsafe.As<TServiceKey, Key>(ref Unsafe.AsRef(in serviceKey));
                var keyedServices = KeyedServices();
                keyedServices.Add(typedServiceKey, service);
            } else {
                throw new NotSupportedException($"The service key is not of type {typeof(Key)}");
            }
        } else {
            throw new NotSupportedException($"The schema version of the key {serviceKey.SchemaVersion} is not supported");
        }
    }

    public CoroutineScope BuildCoroutineScope()
    {
        return new CoroutineScope();
    }
}
