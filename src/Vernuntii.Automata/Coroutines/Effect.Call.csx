using System.Text;
using System.IO;

var (FileName, FileContent) = GenerateEffectCallExtensions(1, 16);
File.WriteAllText(FileName, FileContent);

static (string FileName, string FileContent) GenerateEffectCallExtensions(int from, int to)
{
    var sb = new StringBuilder();
    sb.AppendLine("""
        namespace Vernuntii.Coroutines;

        partial class Effect {
        """);

    for (var currentTo = from; currentTo <= to; currentTo++)
    {
        var count = (currentTo - from) + 1;
        var genericParameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x}"));
        var parameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x} value{x}"));
        var arguments = string.Join(", ", Enumerable.Range(1, count).Select(x => $"value{x}"));
        sb.AppendLine($$"""
                public static Coroutine Call<{{genericParameters}}>(Func<{{genericParameters}}, Coroutine> provider, {{parameters}}) => CallInternal(provider, new Closure<{{genericParameters}}>({{arguments}}));

                public static Coroutine<TResult> Call<{{genericParameters}},TResult>(Func<{{genericParameters}},Coroutine<TResult>> provider, {{parameters}}) => CallInternal<TResult>(provider, new Closure<{{genericParameters}}>({{arguments}}));
            """);
        if (currentTo < to)
        {
            sb.AppendLine();
        }
    }

    sb.AppendLine("}");

    return (
        $"Effect.Call.g.cs",
        sb.ToString());
}
