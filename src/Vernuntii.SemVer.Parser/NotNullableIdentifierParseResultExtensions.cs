﻿using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.SemVer.Parser
{
    /// <summary>
    /// Extension methods for <see cref="INotNullableIdentifierParseResult{T}"/>.
    /// </summary>
    public static class NotNullableIdentifierParseResultExtensions
    {
        /// <summary>
        /// Checks if state has any successful state.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="value"></param>
        public static bool DeconstructSuccess<T>(this INotNullableIdentifierParseResult<T> parseResult, [NotNullWhen(true)] out T? value)
        {
            value = parseResult.Value;

            if (parseResult.Suceeded) {
                if (value is null) {
                    throw new ArgumentNullException(nameof(value), "Altought parse result succeeded its value was null");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if state has any failure.
        /// </summary>
        /// <param name="parseResult"></param>
        /// <param name="value"></param>
        public static bool DeconstructFailure<T>(this INotNullableIdentifierParseResult<T> parseResult, [NotNullWhen(false)] out T? value)
        {
            value = parseResult.Value;

            if (parseResult.Failed) {
                return true;
            }

            if (value is null) {
                throw new ArgumentNullException(nameof(value), "Altought parse result succeeded its value was null");
            }

            return false;
        }
    }
}
