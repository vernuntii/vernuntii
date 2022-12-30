using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Compares two instances of type <see cref="MessageIndicatorBase"/>.
    /// </summary>
    internal class MessageIndicatorBaseEqualityComparer : EqualityComparer<MessageIndicatorBase>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public static new readonly MessageIndicatorBaseEqualityComparer Default = new();

        /// <inheritdoc/>
        public override bool Equals(MessageIndicatorBase? x, MessageIndicatorBase? y) =>
            ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.IndicatorName == y.IndicatorName);

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] MessageIndicatorBase obj) =>
            obj.IndicatorName?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }
}
