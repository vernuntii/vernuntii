namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Indicates that an instance is producing plugin registrations.
    /// </summary>
    public interface IPluginRegistrationProducer
    {
        /// <summary>
        /// Adds a plugin registration consumer that consumes next producing plugin registrations.
        /// </summary>
        /// <param name="consumePluginRegistrationAction">The action is called when a plugin registration is produced.</param>
        /// <returns>If disposed it stops the consumption of producing plugin registrations.</returns>
        IDisposable AddPluginRegistrationConsumer(Action<IPluginRegistration> consumePluginRegistrationAction);
    }
}
