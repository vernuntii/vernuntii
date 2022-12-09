namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Helper methods for <see cref="ISignalCounter"/>.
    /// </summary>
    public static class Signals
    {
        public static bool AnyUnsignaled(IEnumerable<ISignalCounter>? signals)
        {
            if (signals != null) {
                foreach (var signal in signals) {
                    if (signal.SignalCount == 0) {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AllSignaled(IEnumerable<ISignalCounter>? signals) =>
            !AnyUnsignaled(signals);

        public static bool AllSignaled(params ISignalCounter[]? signals) =>
            !AnyUnsignaled(signals);
    }
}
