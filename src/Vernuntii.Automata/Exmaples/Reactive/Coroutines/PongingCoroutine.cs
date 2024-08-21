using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Extensions.Coroutines;

namespace Vernuntii.Exmaples.Reactive.Coroutines;

internal class PongingCoroutine
{
    public Coroutine PongWhenPinged()
    {
        var pingedTrace = __co.Observe(e => e.Every(PingingCoroutine.Pinged));
    }
}
