using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vernuntii.PluginSystem
{
    [Generator]
    public class BeforeOnAfterRegistrationPluginGenerator : IIncrementalGenerator
    {
        //public void Initialize(GeneratorInitializationContext context) { }

        //public void Execute(GeneratorExecutionContext context) {
        //    var attributeSource = @"
        //    namespace HelloWorld
        //    {
        //        public class MyExampleAttribute: System.Attribute {} 
        //    }";
        //    context.AddSource("MyExampleAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
        //}

        private static bool IsEnumDeclarationHavingAtLeastOneAttribute(SyntaxNode syntaxNode) =>
            syntaxNode is EnumDeclarationSyntax { AttributeLists.Count: > 0 };

        private static EnumDeclarationSyntax? ProbeEnumDeclarationSuccessfulOtherwiseNull(GeneratorSyntaxContext syntaxContext)
        {
            var enumNode = (EnumDeclarationSyntax)syntaxContext.Node;
            var semenaticModel = syntaxContext.SemanticModel;
            var attributeListEnumerator = enumNode.AttributeLists.GetEnumerator();

            while (attributeListEnumerator.MoveNext()) {
                var attributeEnumerator = attributeListEnumerator.Current.Attributes.GetEnumerator();

                while (attributeEnumerator.MoveNext()) {
                    var attribute = attributeEnumerator.Current;

                    if (semenaticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol) {
                        continue;
                    }

                    if (attributeSymbol.ContainingType.ToDisplayString() == "Vernuntii.PluginSystem.PluginSourceAttribute") {
                        return enumNode;
                    }
                }
            }

            return null;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(postInitContext => {
                postInitContext.AddSource("UsePluginSourceAttribute.g.cs", SourceText.From("""
                    namespace Vernuntii.PluginSystem
                    {
                        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
                        public class PluginSourceAttribute : Attribute
                        {
                            /// <summary>
                            /// By default <see langword="true"/>. If this feature is enabled
                            /// then it allows the source generator to generate additional
                            /// features. Therefore the target class needs to be
                            /// <see langword="partial"/>.
                            /// </summary>.
                            public bool GenerateSource { get; init; } = true;
                        }
                    }
                    """, Encoding.UTF8));
            });

            IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: (s, _) => IsEnumDeclarationHavingAtLeastOneAttribute(s),
                    transform: (c, _) => ProbeEnumDeclarationSuccessfulOtherwiseNull(c))
                .Where(x => x is not null)!;

            //IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnumDeclarations =
            //    context.CompilationProvider.Combine(enumDeclarations.Collect());

            //IncrementalValuesProvider<<<<<<<<<<<

            //context.RegisterSourceOutput(compilationAndEnumDeclarationList,
            //    static (spc, source) => spc.AddSource("test.g.cs", SourceText.From("test", Encoding.UTF8)));
        }
    }
}
