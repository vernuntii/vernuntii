using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Builder for <see cref="IRegexMessageIndicator"/>.
    /// </summary>
    public class RegexMessageIndicatorBuilder
    {
        /// <summary>
        /// <see langword="true"/> means any variable has been changed.
        /// </summary>
        public bool HasChanged => _withMajorRegex || _withMinorRegex || _withPatchRegex;

        private readonly IRegexMessageIndicator _baseRegexIndiactor;
        private Regex? _majorRegex;
        private bool _withMajorRegex;
        private Regex? _minorRegex;
        private bool _withMinorRegex;
        private Regex? _patchRegex;
        private bool _withPatchRegex;

        /// <summary>
        /// Builds an instance of this type with <paramref name="baseRegexIndiactor"/> as base.
        /// </summary>
        /// <param name="baseRegexIndiactor"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RegexMessageIndicatorBuilder(IRegexMessageIndicator baseRegexIndiactor) =>
            _baseRegexIndiactor = baseRegexIndiactor ?? throw new ArgumentNullException(nameof(baseRegexIndiactor));

        /// <summary>
        /// Builds an instance of this type.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public RegexMessageIndicatorBuilder() =>
            _baseRegexIndiactor = RegexMessageIndicator.Default;

        /// <summary>
        /// With major RegEx.
        /// </summary>
        /// <param name="majorRegex"></param>
        public RegexMessageIndicatorBuilder MajorRegex(Regex? majorRegex)
        {
            _majorRegex = majorRegex;
            _withMajorRegex = true;
            return this;
        }

        /// <summary>
        /// With minor RegEx.
        /// </summary>
        /// <param name="minorRegex"></param>
        public RegexMessageIndicatorBuilder MinorRegex(Regex? minorRegex)
        {
            _minorRegex = minorRegex;
            _withMinorRegex = true;
            return this;
        }

        /// <summary>
        /// With patch RegEx.
        /// </summary>
        /// <param name="patchRegex"></param>
        public RegexMessageIndicatorBuilder PatchRegex(Regex? patchRegex)
        {
            _patchRegex = patchRegex;
            _withPatchRegex = true;
            return this;
        }

        /// <summary>
        /// Builds the new RegEx message indicator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T ToIndicator<T>()
            where T : RegexMessageIndicatorBase, new()
        {
            return new T() {
                MajorRegex = _withMajorRegex ? _majorRegex : _baseRegexIndiactor?.MajorRegex,
                MinorRegex = _withMinorRegex ? _minorRegex : _baseRegexIndiactor?.MinorRegex,
                PatchRegex = _withPatchRegex ? _patchRegex : _baseRegexIndiactor?.PatchRegex
            };
        }

        /// <summary>
        /// Builds the new RegEx message indicator.
        /// </summary>
        public IRegexMessageIndicator ToIndicator()
        {
            if (!HasChanged) {
                return _baseRegexIndiactor;
            }

            return ToIndicator<RegexMessageIndicator>();
        }
    }
}
