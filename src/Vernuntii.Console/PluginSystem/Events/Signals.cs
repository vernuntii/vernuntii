namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Helper methods for <see cref="IOneSignal"/>.
    /// </summary>
    public static class Signals
    {
        public static bool AnyUnsignaled(IEnumerable<IOneSignal>? signals)
        {
            if (signals != null) {
                foreach (var signal in signals) {
                    if (!signal.SignaledOnce) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AllSignaled(IEnumerable<IOneSignal>? signals) =>
            !AnyUnsignaled(signals);

        public static bool AllSignaled(params IOneSignal[]? signals) =>
            !AnyUnsignaled(signals);
    }
}
