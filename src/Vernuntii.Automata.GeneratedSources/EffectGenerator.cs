using System.Text;

namespace Vernuntii.Automata.GeneratedSources;

[Generator]
public class EffectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context => {
            var closureClass = GenerateClosureClass(1, 16);
            context.AddSource(closureClass.FileName, closureClass.FileContent);
            var callClass = GenerateEffectCallExtensions(1, 16);
            context.AddSource(callClass.FileName, callClass.FileContent);
        });

        static (string FileName, string FileContent) GenerateClosureClass(int from, int to)
        {
            var sb = new StringBuilder();
            sb.AppendLine("""
                using System.Runtime.CompilerServices;

                namespace Vernuntii.Coroutines;

                """);

            for (var currentTo = from ; currentTo <= to; currentTo++) {
                var count = (currentTo - from) + 1;
                var genericParameters = string.Join(",", Enumerable.Range(1, count).Select(x => $"T{x}"));
                var parameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x} Value{x}"));
                var arguments = string.Join(", ", Enumerable.Range(1, count).Select(x => $"Value{x}"));
                sb.Append($$"""
                    public sealed record Closure<{{genericParameters}}>({{parameters}}) : IClosure
                    {
                        TResult IClosure.InvokeClosured<TResult>(Delegate delegateReference)
                        {
                            return Unsafe.As<Func<{{genericParameters}},TResult>>(delegateReference).Invoke({{arguments}});
                        }
                    }
                    """);
                sb.AppendLine();
            }

            return (
                $"Closure.Generic.g.cs",
                sb.ToString());
        }

        static (string FileName, string FileContent) GenerateEffectCallExtensions(int from, int to)
        {
            var sb = new StringBuilder();
            sb.AppendLine("""
                namespace Vernuntii.Coroutines;

                partial class Effect {
                """);

            for (var currentTo = from; currentTo <= to; currentTo++) {
                var count = (currentTo - from) + 1;
                var genericParameters = string.Join(",", Enumerable.Range(1, count).Select(x => $"T{x}"));
                var parameters = string.Join(", ", Enumerable.Range(1, count).Select(x => $"T{x} value{x}"));
                var arguments = string.Join(", ", Enumerable.Range(1, count).Select(x => $"value{x}"));
                sb.Append($$"""
                        public static Coroutine Call<{{genericParameters}}>(Func<{{genericParameters}}, Coroutine> provider, {{parameters}}) => CallInternal(provider, new Closure<{{genericParameters}}>({{arguments}}));

                        public static Coroutine<TResult> Call<{{genericParameters}},TResult>(Func<{{genericParameters}},Coroutine<TResult>> provider, {{parameters}}) => CallInternal(provider, new Closure<{{genericParameters}}>({{arguments}}));

                    """);
            }

            sb.AppendLine("}");

            return (
                $"Effect.Call.Generic.g.cs",
                sb.ToString());
        }
    }
}
