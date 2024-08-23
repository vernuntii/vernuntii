using System.Runtime.CompilerServices;
using Vernuntii.Coroutines.v1;

namespace Vernuntii.Coroutines;

public class CoroutineScopeBuilder
{
    private Dictionary<Key, object>? _keyedServices;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Dictionary<Key, object> GetKeyedServices() => _keyedServices ??= [];

    public void AddKeyedService<TServiceKey, TService>(in TServiceKey serviceKey, TService service)
        where TServiceKey : IKey
        where TService : class
    {
        if (serviceKey.SchemaVersion == 1) {
            if (default(TServiceKey) != null && serviceKey is Key) {
                ref var typedServiceKey = ref Unsafe.As<TServiceKey, Key>(ref Unsafe.AsRef(serviceKey));
                var keyedServices = GetKeyedServices();
                keyedServices.Add(typedServiceKey, service);
            } else {
                throw new NotSupportedException($"The key is not of type {typeof(Key)}");
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
