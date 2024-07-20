using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vernuntii.Automata.Generator;

public class MySyntaxReceiver : ISyntaxReceiver
{


    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax methodDeclaration && methodDeclaration.ReturnType is IdentifierNameSyntax returnType && returnType.Identifier.Text == "Coroutine") {
            ;
        }
    }
}

[Generator]
public class Generator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) => throw new NotImplementedException();
}
