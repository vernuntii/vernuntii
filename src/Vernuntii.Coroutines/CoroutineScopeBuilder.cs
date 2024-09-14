using System.Runtime.CompilerServices;

namespace Vernuntii.Coroutines;

public class CoroutineScopeBuilder
{
    private Dictionary<Key, object>? _keyedServices;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Dictionary<Key, object> KeyedServices() => _keyedServices ??= [];

    public void AddKeyedService<TServiceKey, TService>(in Key serviceKey, TService service)
        where TService : class
    {
        if (serviceKey.SchemaVersion == 1) {
            var keyedServices = KeyedServices();
            keyedServices.Add(serviceKey, service);
        } else {
            throw KeyThrowHelper.SchemaVersionNotSupported(serviceKey);
        }
    }

    public CoroutineScope BuildCoroutineScope()
    {
        return new CoroutineScope();
    }
}
