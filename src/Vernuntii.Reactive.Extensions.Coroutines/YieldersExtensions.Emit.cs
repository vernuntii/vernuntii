using System.Runtime.CompilerServices;
using Vernuntii;
using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class YieldersExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coroutine Emit<T>(this __co_ext _, IEventDiscriminator<T> eventDiscriminator, T eventData) =>
        Yielders.Emit(eventDiscriminator, eventData);
}
