using Nito.Comparers;

namespace Vernuntii.Coroutines.GeneratedSources;

internal static class VernuntiiEqualityComparerBuilder
{
    public static EqualityComparerBuilderFor<T> ForElementsOf<T>(IncrementalValuesProvider<T> provider) => EqualityComparerBuilderFor<T>.Instance;
}
