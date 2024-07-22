using Nito.Comparers;

namespace Vernuntii.Automata.Generator;

[Generator]
public class EffectResultGenerator : IIncrementalGenerator
{
    private static Random s_random = new Random();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax { ReturnType: IdentifierNameSyntax { Identifier.Text: "Coroutine" }, Body: { } },
                transform: static (x, cancellationToken) => {
                    var method = (MethodDeclarationSyntax)x.Node;

                    return new {
                        x.SemanticModel,
                        CoroutineMethodDeclaration = method,
                    };
                });

        var methodComparer = VernuntiiEqualityComparerBuilder.ForElementsOf(methods)
            .EquateBy(x => x?.CoroutineMethodDeclaration.Identifier.Text)
            .ThenEquateBy(x => x?.CoroutineMethodDeclaration.Body!.Statements.Aggregate(0L, (result, x) => result + x.Span.Length));

        var methodsWithComparer = methods.WithComparer(methodComparer);

        var methodStatements = methodsWithComparer.SelectMany(static (x, _) => x.CoroutineMethodDeclaration.Body!.Statements
                .SelectMany(x => x
                    .DescendantNodesAndSelf()
                    .OfType<InvocationExpressionSyntax>())
                    .Where(static x => x is {
                        Expression: GenericNameSyntax {
                            Identifier.Text: "All",
                            TypeArgumentList.Arguments: { Count: 1 }
                        },
                        ArgumentList.Arguments: { Count: 1 } and [{ Expression: AnonymousObjectCreationExpressionSyntax }]
                    })
                    .Select(y => new {
                        x.SemanticModel,
                        x.CoroutineMethodDeclaration,
                        InvocationExpression = y,
                    }));

        var methodStatementComparer = VernuntiiEqualityComparerBuilder.ForElementsOf(methodStatements)
            .EquateBy(x => x?.InvocationExpression.Span.Length);

        var methodStatementsWithComparer = methodStatements.WithComparer(methodStatementComparer);

        var invocations = methodStatementsWithComparer.Select(static (x, cancellationToken) => {
            var coroutineMethodSymbol = x.SemanticModel.GetDeclaredSymbol(x.CoroutineMethodDeclaration, cancellationToken);
            var invocationInnerExpression = (GenericNameSyntax)x.InvocationExpression.Expression;
            var workContextType = x.SemanticModel.GetTypeInfo(invocationInnerExpression.TypeArgumentList.Arguments[0], cancellationToken).Type;
            var workCreationExpression = (AnonymousObjectCreationExpressionSyntax)x.InvocationExpression.ArgumentList.Arguments[0].Expression;

            return new {
                x.SemanticModel,
                x.CoroutineMethodDeclaration,
                CoroutineMethodSymbol = coroutineMethodSymbol,
                x.InvocationExpression,
                InvocationInnerExpression = invocationInnerExpression,
                WorkContextType = workContextType,
                WorkCreationExpression = workCreationExpression,
            };
        })
            .Where(static x => x is { CoroutineMethodSymbol: { }, WorkContextType: { TypeKind: TypeKind.Error } })
            .Select(static (x, cancellationToken) => {
                var namespaceSymbol = x.CoroutineMethodSymbol!.ContainingNamespace;
                var containingTypes = GetContainingTypes(x.SemanticModel, x.CoroutineMethodSymbol);
                var workCreationExpressionProperties = GetAllEffectObjectCreationProperties(x.SemanticModel, x.WorkCreationExpression);

                var fileName = Path.GetFileNameWithoutExtension(x.CoroutineMethodDeclaration.SyntaxTree.FilePath);
                var fileSuffix = RandomString(16);
                var resourceNameHint = $"{fileName}_{fileSuffix}.g.cs";

                return new {
                    ResourceNameHint = resourceNameHint,
                    NamespaceSymbol = namespaceSymbol,
                    x.SemanticModel,
                    ContainingTypes = containingTypes,
                    x.CoroutineMethodDeclaration,
                    x.CoroutineMethodSymbol,
                    x.InvocationExpression,
                    WorkContextType = x.WorkContextType!,
                    x.WorkCreationExpression,
                    WorkCreationExpressionProperties = workCreationExpressionProperties,
                };
            });

        // TODO: Implement anti duplication mechanism when same work context type in parent node

        context.RegisterSourceOutput(invocations, (ctx, src) => {
            var workClass = src.WorkContextType.Name;

            var metaProperties = string.Join(", ", src.WorkCreationExpressionProperties.Select(property => $"""
                new AllEffectResultProperty(propertyName: "{property.PropertyName}", isAwaitable: {(property.AwaitingType is null ? "false" : "true")})
                """));

            var allEffectResultClassProperties = string.Join("\n", src.WorkCreationExpressionProperties.Select(property => $$"""
                    public {{(property.AwaitingType ?? property.PropertyType).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} {{property.PropertyName}} { get; set; }
                """));

            var allEffectResultClass = $$"""
                    public class {{workClass}} : Vernuntii.Reactive.Coroutines.AsyncEffects.IAllEffectResult {
                        private readonly static AllEffectResultProperty[] s_properties = new AllEffectResultProperty[] { {{metaProperties}} };
                    
                        AllEffectResultProperty[] IAllEffectResult.FirstLevelProperties => s_properties;
                    
                    {{allEffectResultClassProperties}}
                    }
                    """;

            var memberContent = src.ContainingTypes.Count switch {
                0 => allEffectResultClass,
                _ => src.ContainingTypes.Aggregate(string.Empty, (result, containingType) => {
                    var typeDeclaration = containingType.DeclaringSyntaxReferences
                        .Select(x => x.GetSyntax())
                        .OfType<TypeDeclarationSyntax>()
                        .First();

                    var keyword = typeDeclaration.Kind() switch {
                        SyntaxKind.InterfaceDeclaration => "interface",
                        SyntaxKind.ClassDeclaration => "class",
                        SyntaxKind.RecordDeclaration => "record",
                        (SyntaxKind)9068 => "record struct", // RecordStructDeclaration
                        SyntaxKind.StructDeclaration => "struct",
                        var kind => throw new ArgumentOutOfRangeException($"Syntax kind {kind} not supported")
                    };

                    var typeName = containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    var typeKindWithNameAndModifier = $"partial {keyword} {typeName}";

                    if (result.Length == 0) {
                        return $$"""
                        {{typeKindWithNameAndModifier}} {
                        {{allEffectResultClass}}
                        }
                        """;
                    }

                    return $$"""
                    {{typeKindWithNameAndModifier}} {
                    {{result}}
                    }
                    """;
                })
            };

            var namespaceName = src.NamespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            var content = $$"""
                #nullable disable
                namespace {{namespaceName}};

                {{memberContent}}
                #nullable enable
                """;

            ctx.AddSource(src.ResourceNameHint, content);
        });


        static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[s_random.Next(s.Length)]).ToArray());
        }

        static List<INamedTypeSymbol> GetContainingTypes(SemanticModel semanticModel, ISymbol declaration)
        {
            var containingSymbol = declaration.ContainingType;
            var containingSymbolStack = new List<INamedTypeSymbol>();

            while (containingSymbol is not null) {
                if (containingSymbol is not { TypeKind: TypeKind.Class or TypeKind.Interface or TypeKind.Struct }) {
                    containingSymbolStack.Clear();
                }

                containingSymbolStack.Add(containingSymbol);
                containingSymbol = containingSymbol.ContainingType;
            }

            return containingSymbolStack;
        }

        static List<AllEffectResultProperty> GetAllEffectObjectCreationProperties(SemanticModel semanticModel, AnonymousObjectCreationExpressionSyntax workExpression)
        {
            var infos = new List<AllEffectResultProperty>();

            foreach (var memberDeclarator in workExpression.Initializers) {
                if (memberDeclarator is not AnonymousObjectMemberDeclaratorSyntax { NameEquals: { } }) {
                    continue;
                }

                var propertyName = memberDeclarator.NameEquals.Name.Identifier.Text;
                var expression = memberDeclarator.Expression;

                var propertyTypeInfo = semanticModel.GetTypeInfo(expression);
                var propertyType = propertyTypeInfo.Type;

                if (propertyType is null) {
                    continue;
                }

                var awaiterMethod = propertyType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Name == "GetAwaiter");

                var awaitingType = awaiterMethod?.ReturnType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Name == "GetResult" && m.Parameters.Length == 0)?.ReturnType;

                infos.Add(new AllEffectResultProperty(
                    PropertyName: propertyName,
                    PropertyType: propertyType,
                    AwaitingType: awaitingType
                ));
            }

            return infos;
        }
    }
}
