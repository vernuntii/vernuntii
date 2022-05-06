using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vernuntii.Git.Command
{
    internal readonly struct CultureStringBuilder
    {
        public static CultureStringBuilder Invariant() =>
            new CultureStringBuilder(CultureInfo.InvariantCulture);

        private readonly StringBuilder _stringBuilder;
        private readonly CultureInfo _culture;

        public CultureStringBuilder(StringBuilder stringBuilder, CultureInfo culture)
        {
            _stringBuilder = stringBuilder;
            _culture = culture;
        }

        public CultureStringBuilder(CultureInfo culture)
        {
            _stringBuilder = new StringBuilder();
            _culture = culture;
        }


        /// <summary>
        /// Appends a copy of the specified string to this instance.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The string to append.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Append(string? value) =>
            _stringBuilder.Append(value);

        /// <summary>
        /// Appends the specified interpolated string to this instance.
        /// </summary>
        /// <param name="handler">The interpolated string to append.</param>
        public void Append([InterpolatedStringHandlerArgument("")] ref StringBuilder.AppendInterpolatedStringHandler handler) =>
            _stringBuilder.Append(_culture, ref handler);

        /// <summary>
        /// Converts the value of this instance to a System.String.
        /// </summary>
        public override string ToString() =>
            _stringBuilder.ToString();

        public static implicit operator StringBuilder(CultureStringBuilder stringBuilder) =>
            stringBuilder._stringBuilder;

        public static implicit operator string(CultureStringBuilder stringBuilder) =>
            stringBuilder.ToString();
    }
}
