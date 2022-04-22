namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Extension methods for <see cref="IPluginEventAggregator"/>.
    /// </summary>
    public static class PluginEventAggregatorExtensions
    {
        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        public static void PublishEvent<TEvent>(this IPluginEventAggregator eventAggregator)
            where TEvent : PubSubEvent, new() =>
            eventAggregator.GetEvent<TEvent>().Publish();

        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        public static void PublishEvent<TEvent, TPayload>(this IPluginEventAggregator eventAggregator, TPayload payload)
            where TEvent : PubSubEvent<TPayload>, new() =>
            eventAggregator.GetEvent<TEvent>().Publish(payload);

        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TPayload"></typeparam>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void PublishEvent<TEvent, TPayload>(this IPluginEventAggregator eventAggregator, TEvent discriminator, TPayload payload)
            where TEvent : PubSubEvent<TPayload>, new() =>
            eventAggregator.GetEvent<TEvent>().Publish(payload);
    }
}
