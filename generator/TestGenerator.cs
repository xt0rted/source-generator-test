namespace TestGenerator;

using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class TestGenerator : IIncrementalGenerator
{
    private const string EnumExtensionsAttribute = "BooleanFlagsAttribute";

    private static readonly DiagnosticDescriptor _errorDescriptor = new(
        "SI0000",
        "Error in the TestGenerator generator",
        "Error in the TestGenerator generator: '{0}'",
        "TestGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();

        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is EnumDeclarationSyntax d && d.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(d => d is not null)!;

        var compilationAndEnums = context.CompilationProvider.Combine(enumDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndEnums, (spc, source) =>
        {
            try
            {
                Execute(source.Left, source.Right, spc);
            }
            catch (Exception ex)
            {
                spc.ReportDiagnostic(Diagnostic.Create(_errorDescriptor, Location.None, ex.ToString()));
            }
        });
    }

    private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
    {
        foreach (var enumDeclaration in enums)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(enumDeclaration.SyntaxTree);

            if (semanticModel.GetDeclaredSymbol(enumDeclaration) is not INamedTypeSymbol enumSymbol)
            {
                continue;
            }

            var className = enumSymbol.Name + "Flags";
            var sb = new StringBuilder();

            if (!enumSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                sb.Append("namespace ");
                sb.Append(enumSymbol.ContainingNamespace.ToString());
                sb.AppendLine(";");
                sb.AppendLine();
            }

            sb.Append("public class ");
            sb.AppendLine(className);
            sb.AppendLine("{");

            for (int i = 0; i < enumDeclaration.Members.Count; i++)
            {
                var info = semanticModel.GetDeclaredSymbol(enumDeclaration.Members[i])!;

                sb.AppendLine($"    public bool {info.Name} {{ get; set; }} ");
            }

            sb.AppendLine("}");

            context.AddSource(className, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;

                if (attributeContainingTypeSymbol.Name == EnumExtensionsAttribute)
                {
                    return enumDeclarationSyntax;
                }
            }
        }

        return null;
    }
}
