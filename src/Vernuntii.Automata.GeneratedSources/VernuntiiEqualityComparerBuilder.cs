using Nito.Comparers;

namespace Vernuntii.Automata.GeneratedSources;

internal static class VernuntiiEqualityComparerBuilder
{
    public static EqualityComparerBuilderFor<T> ForElementsOf<T>(IncrementalValuesProvider<T> provider) => EqualityComparerBuilderFor<T>.Instance;
}
