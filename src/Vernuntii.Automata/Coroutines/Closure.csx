using System.Text;
using System.IO;

var (FileName, FileContent) = GenerateClosureClass(1, 16);
File.WriteAllText(FileName, FileContent);
 
static (string FileName, string FileContent) GenerateClosureClass(int from, int to)
{
    var sb = new StringBuilder();
    sb.AppendLine("""
                using System.Runtime.CompilerServices;

                namespace Vernuntii.Coroutines;

                """);

    for (var currentTo = from; currentTo <= to; currentTo++)
    {
        var count = (currentTo - from) + 1;
        var genericParameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x}"));
        var parameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x} Value{x}"));
        var arguments = string.Join(", ", Enumerable.Range(1, count).Select(x => $"Value{x}"));
        sb.AppendLine($$"""
                    public sealed record Closure<{{genericParameters}}>({{parameters}}) : IClosure
                    {
                        public int Length => {{count}};

                        TResult IClosure.InvokeDelegateWithClosure<TResult>(Delegate delegateToInvoke)
                        {
                            return Unsafe.As<Func<{{genericParameters}},TResult>>(delegateToInvoke).Invoke({{arguments}});
                        }
                    }
                    """);
        if (currentTo < to)
        {
            sb.AppendLine();
        }
    }

    return (
        $"Closure.g.cs",
        sb.ToString());
}
