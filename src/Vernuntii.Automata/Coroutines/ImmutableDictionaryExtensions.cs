using System.Collections.Immutable;

namespace Vernuntii.Coroutines;

internal static class ImmutableDictionaryExtensions
{
    /// <summary>
    /// Merges the right dictionary into the left dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static ImmutableDictionary<TKey,TValue> Merge<TKey, TValue>(this ImmutableDictionary<TKey, TValue> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
    {
        var builder = left.ToBuilder();

        foreach (var pair in right) {
            builder[pair.Key] = pair.Value;
        }

        return builder.ToImmutable();
    }
}
