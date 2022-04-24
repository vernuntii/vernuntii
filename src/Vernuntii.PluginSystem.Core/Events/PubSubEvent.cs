using Prism.Events;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Defines a class that manages publication and subscription to events.
    /// </summary>
    public class PubSubEvent : Prism.Events.PubSubEvent { }

    /// <summary>
    /// Defines a class that manages publication and subscription to events.
    /// </summary>
    public class PubSubEvent<TPayload> : Prism.Events.PubSubEvent<TPayload> { }

    /// <summary>
    /// Defines a class that manages publication and subscription to
    /// events. Serves the purpose to hold the generic information about
    /// <typeparamref name="TDerived"/> to allow its infererence in
    /// <see cref="Plugin.SubscribeEvent{TEvent, TPayload}(PubSubEvent{TEvent, TPayload}, Action{TPayload})"/>.
    /// </summary>
    /// <typeparam name="TDerived"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public class PubSubEvent<TDerived, TPayload> : PubSubEvent<TPayload>
        where TDerived : PubSubEvent<TDerived, TPayload>, new()
    {
        /// <summary>
        /// The discrimnator to enable inference of <typeparamref name="TPayload"/>.
        /// </summary>
        public readonly static TDerived Discriminator = new TDerived();
    }
}
