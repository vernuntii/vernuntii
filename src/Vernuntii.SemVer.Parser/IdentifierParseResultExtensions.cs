using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Extension methods for <see cref="IIdentifierParseResult{T}"/>.
    /// </summary>
    public static class IdentifierParseResultExtensions
    {
        /// <summary>
        /// Checks if state has any successful state.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        public static bool DeconstructSuccess<T>(this IIdentifierParseResult<T> parseResult, [NotNullIfNotNull("defaultValue")] out T? value, T defaultValue)
        {
            if (parseResult.Value is null) {
                value = defaultValue;
            } else {
                value = parseResult.Value;
            }

            if (parseResult.Failed) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if state has any failure.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        public static bool DeconstructFailure<T>(this IIdentifierParseResult<T> parseResult, [NotNullIfNotNull("defaultValue")] out T? value, T defaultValue)
        {
            if (parseResult.Value is null) {
                value = defaultValue;
            } else {
                value = parseResult.Value;
            }

            if (parseResult.Failed) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deconstructs this instance.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="state"></param>
        /// <param name="value"></param>
        public static void Deconstruct<T>(this IIdentifierParseResult<T> parseResult, out IdentifierParseResultState state, out T? value)
        {
            state = parseResult.State;
            value = parseResult.Value;
        }

        /// <summary>
        /// Deconstruct this instance.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="value"></param>
        public static IdentifierParseResultState Deconstruct<T>(this IIdentifierParseResult<T> parseResult, out T? value)
        {
            value = parseResult.Value;
            return parseResult.State;
        }
    }
}
