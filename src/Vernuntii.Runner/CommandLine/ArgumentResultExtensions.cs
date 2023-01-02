using System.Collections.Immutable;
using System.CommandLine.Parsing;
using Vernuntii.Collections;

namespace Vernuntii.CommandLine;

internal static class ArgumentResultExtensions
{
    /// <summary>
    /// Parses <paramref name="result"/> as comma-separated string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="valueAllowList"></param>
    /// <param name="valueFactory"></param>
    /// <param name="allowEmpty"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static IReadOnlyList<T> ParseList<T>(this ArgumentResult result, Func<string, T> valueFactory, IReadOnlyContentwiseCollection<T>? valueAllowList = null, bool allowEmpty = false)
    {
        if (result is null) {
            throw new ArgumentNullException(nameof(result));
        }

        if (valueAllowList is null) {
            throw new ArgumentNullException(nameof(valueAllowList));
        }

        if (valueFactory is null) {
            throw new ArgumentNullException(nameof(valueFactory));
        }

        if (result.Tokens.Count == 0) {
            if (!allowEmpty) {
                result.ErrorMessage = "At least one enum value is required.";
            }

            return ImmutableList<T>.Empty;
        }

        var values = new List<T>();
        var tokenEnumerator = result.Tokens.GetEnumerator();

        while (tokenEnumerator.MoveNext()) {
            var token = tokenEnumerator.Current;
            var valueEnumerator = token.Value.Split(",").Select(valueFactory).GetEnumerator();

            while (valueEnumerator.MoveNext()) {
                var value = valueEnumerator.Current;

                if (valueAllowList is not null && !valueAllowList.TryGetValue(value, out value)) {
                    result.ErrorMessage = $"The '{value}' value is not allowed";
                    return ImmutableList<T>.Empty;
                }

                values.Add(value);
            }
        }

        return values;
    }

    public static bool IsParentOptionTokenEquals(this SymbolResult result, string token) =>
        result.Parent is OptionResult optionsResult && StringComparer.InvariantCulture.Equals(optionsResult.Token?.Value, token);
}
