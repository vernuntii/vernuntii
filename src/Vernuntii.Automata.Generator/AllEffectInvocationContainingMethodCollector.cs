using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vernuntii.Automata.Generator;

public record AllEffectInvocation(
    BaseNamespaceDeclarationSyntax NamespaceDeclaration,
    MethodDeclarationSyntax ParentMethodDeclaration,
    InvocationExpressionSyntax InvocationExpression,
    AnonymousObjectCreationExpressionSyntax WorkExpression,
    IdentifierNameSyntax WorkContextName);

public record AllEffectInvocationContainingMethod(
    MethodDeclarationSyntax MethodDeclaration,
    IReadOnlyList<AllEffectInvocation> Invocations);

public record AllEffectInvocationProperty(string PropertyName, ITypeSymbol? AwaitingType);

public record AllEffectInvocationWithModel(
    string Namespace,
    AllEffectInvocationContainingMethod Method,
    SemanticModel MethodSemanticModel,
    INamedTypeSymbol ParentMethodContainingType,
    AllEffectInvocation Invocation,
    IReadOnlyList<AllEffectInvocationProperty> Properties);

public static class Helper
{
    public static string? GetNamespace(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration)
    {
        var containingNamespace = GetNamespaceDeclaration(methodDeclaration);

        if (containingNamespace != null) {
            var namespaceSymbol = semanticModel.GetDeclaredSymbol(containingNamespace) as INamespaceSymbol;
            return namespaceSymbol?.ToDisplayString();
        }

        return null;
    }

    public static BaseNamespaceDeclarationSyntax? GetNamespaceDeclaration(SyntaxNode? node)
    {
        while (node != null) {
            if (node is NamespaceDeclarationSyntax namespaceDeclaration) {
                return namespaceDeclaration;
            } else if (node is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration) {
                return fileScopedNamespaceDeclaration;
            }

            node = node.Parent;
        }

        return null;
    }

    public static MethodDeclarationSyntax? GetMethodDeclaration(SyntaxNode? node)
    {
        while (node != null) {
            if (node is MethodDeclarationSyntax namespaceDeclaration) {
                return namespaceDeclaration;
            }

            node = node.Parent;
        }

        return null;
    }
}

public class AllEffectInvocationContainingMethodCollector : ISyntaxReceiver
{
    public List<AllEffectInvocationContainingMethod> AllEffectInvocationContainingMethods { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax method && method.ReturnType is IdentifierNameSyntax returnType && returnType.Identifier.Text == "Coroutine") {
            var statements = method.Body?.Statements;

            if (statements is null) {
                return;
            }

            var invocations = new List<AllEffectInvocation>();

            foreach (var statement in statements) {
                foreach (var invocation in statement.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>()) {
                    if (invocation.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "All") {
                        var arguments = invocation.ArgumentList.Arguments;

                        var namespaceDeclaration = Helper.GetNamespaceDeclaration(invocation);

                        if (namespaceDeclaration is null) {
                            continue;
                        }

                        var parentMethodDeclaration = Helper.GetMethodDeclaration(invocation);

                        if (parentMethodDeclaration is null) {
                            continue;
                        }

                        if (!(arguments.Count == 2 &&
                            arguments[0].Expression is AnonymousObjectCreationExpressionSyntax work &&
                            arguments[1].Expression is IdentifierNameSyntax workContext)) {
                            continue;
                        }

                        invocations.Add(new AllEffectInvocation(namespaceDeclaration, parentMethodDeclaration, invocation, work, workContext));
                    }
                }
            }

            AllEffectInvocationContainingMethods.Add(new AllEffectInvocationContainingMethod(method, invocations));
        }
    }
}


[Generator]
public class Generator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AllEffectInvocationContainingMethodCollector());
    }

    private static List<AllEffectInvocationProperty> GetAllEffectInvocationProperties(SemanticModel semanticModel, AllEffectInvocation invocation)
    {
        var infos = new List<AllEffectInvocationProperty>();

        foreach (var initializer in invocation.WorkExpression.Initializers) {
            if (initializer is AnonymousObjectMemberDeclaratorSyntax memberDeclarator && memberDeclarator.NameEquals != null) {
                var propertyName = memberDeclarator.NameEquals.Name.Identifier.Text;
                var expression = memberDeclarator.Expression;

                var propertyTypeInfo = semanticModel.GetTypeInfo(expression);
                var propertyType = propertyTypeInfo.Type;

                var awaiterMethod = propertyType?
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Name == "GetAwaiter");

                var awaitingType = awaiterMethod?.ReturnType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Name == "GetResult" && m.Parameters.Length == 0)?.ReturnType;

                infos.Add(new AllEffectInvocationProperty(
                    PropertyName: propertyName,
                    AwaitingType: awaitingType
                ));
            }
        }

        return infos;
    }


    public void WriteAllEffectWorkResultingClass(AllEffectInvocationWithModel invocation, StringBuilder sb)
    {
        //sb.

        //sb.AppendLine($"""
        //    namespace {invocation.Method.}
        //    """);
    }

    public static Random random = new Random();

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AllEffectInvocationContainingMethodCollector collector) {
            return;
        }

        var listOfInvocationsPerMethod = collector.AllEffectInvocationContainingMethods.Select((method => {
            var methodSemanticModel = context.Compilation.GetSemanticModel(method.MethodDeclaration.SyntaxTree);

            return method.Invocations.Aggregate(new List<AllEffectInvocationWithModel>(), (result, invocation) => {
                var invocationContainingType = methodSemanticModel.GetDeclaredSymbol(invocation.ParentMethodDeclaration)?.ContainingType;
                var @namespace = Helper.GetNamespace(methodSemanticModel, method.MethodDeclaration);

                if (@namespace is not null && invocationContainingType is not null) {
                    result.Add(new AllEffectInvocationWithModel(
                        @namespace,
                        method,
                        methodSemanticModel,
                        invocationContainingType,
                        invocation,
                        GetAllEffectInvocationProperties(methodSemanticModel, invocation)));
                }

                return result;
            });
        })).SelectMany(x => x);

        foreach (var invocation in listOfInvocationsPerMethod) {
            var fileName = Path.GetFileNameWithoutExtension(invocation.Invocation.InvocationExpression.SyntaxTree.FilePath);
            var fileSuffix = RandomString(fileName.Length);
            var containingType = invocation.ParentMethodContainingType;
            var containingTypeLiteral = containingType.TypeKind switch {
                TypeKind.Class => "class",
                TypeKind.Interface => "interface",
                _ => throw new NotImplementedException()
            };
            var workClass = invocation.Invocation.WorkContextName;
            var content = $$"""
                namespace {{invocation.Namespace}};

                partial {{containingTypeLiteral}} {{containingType.Name}} {
                    public class {{workClass}} {
                        
                    }
                }
                """;
            context.AddSource($"{fileName}_{fileSuffix}.g.cs", content);
        }
    }
}
