namespace Vernuntii.MessageVersioning.MessageIndicators
{
    /// <summary>
    /// Message indicator name comparer.
    /// </summary>
    public class MessageIndicatorNameComparer : EqualityComparer<IMessageIndicator>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public new static readonly MessageIndicatorNameComparer Default = new MessageIndicatorNameComparer();

        /// <inheritdoc/>
        public override bool Equals(IMessageIndicator? x, IMessageIndicator? y) =>
            ReferenceEquals(x, y) || string.Equals(x?.IndicatorName, y?.IndicatorName, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override int GetHashCode(IMessageIndicator obj) =>
            obj.IndicatorName.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}
