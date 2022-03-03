namespace Vernuntii.MessageVersioning.MessageIndicators
{
    internal class MessageIndicatorRegistry
    {
        public MessageIndicatorRegistry Inbuilt = Create(
            ConventionalCommitsMessageIndicator.Default,
            FalsyMessageIndicator.Default,
            TruthyMessageIndicator.Default);

        public static MessageIndicatorRegistry Create(params IMessageIndicator[] messageIndicators)
        {
            var registry = new MessageIndicatorRegistry();
            registry.AddRange(messageIndicators);
            return registry;
        }

        private Dictionary<string, IMessageIndicator> _registeredMessageIndicators;

        public MessageIndicatorRegistry() =>
            _registeredMessageIndicators = new Dictionary<string, IMessageIndicator>(StringComparer.OrdinalIgnoreCase);

        public void Add(IMessageIndicator messageIndicator) =>
            _registeredMessageIndicators.Add(messageIndicator.IndicatorName, messageIndicator);

        public void AddRange(IEnumerable<IMessageIndicator> messageIndicators)
        {
            foreach (var messageIndicator in messageIndicators) {
                Add(messageIndicator);
            }
        }

        public IMessageIndicator? GetOrDefault(string indicatorName) =>
            _registeredMessageIndicators.GetValueOrDefault(indicatorName);
    }
}
