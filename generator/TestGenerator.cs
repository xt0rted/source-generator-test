namespace TestGenerator;

using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class TestGenerator : ISourceGenerator
{
    private const string EnumExtensionsAttribute = "BooleanFlagsAttribute";

    private static readonly DiagnosticDescriptor _errorDescriptor = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "SI0000",
#pragma warning restore RS2008 // Enable analyzer release tracking
            "Error in the IconSourceGenerator generator",
    "Error in the IconSourceGenerator generator: '{0}'",
    "IconSourceGenerator",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            ExecuteInternal(context);
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(_errorDescriptor, Location.None, ex.ToString()));
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ExecuteInternal(GeneratorExecutionContext context)
    {
        var receiver = (ServicesReceiver?)context.SyntaxReceiver;
        if (receiver == null || !receiver.EnumsToGenerate.Any())
        {
            return;
        }


        foreach (var @enum in receiver.EnumsToGenerate)
        {
            var semanticModel = context.Compilation.GetSemanticModel(@enum.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(@enum);

            var generateAttribute = symbol.GetAttributes().FirstOrDefault(at => at.AttributeClass?.Name == EnumExtensionsAttribute);
            if (generateAttribute is null)
            {
                continue;
            }

            var sb = new StringBuilder();

            var ns = symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToString();
            if (ns != null)
            {
                sb.Append("namespace ");
                sb.Append(ns);
                sb.AppendLine(";");
                sb.AppendLine();
            }

            var className = symbol.Name + "Flags";

            sb.Append("public class ");
            sb.AppendLine(className);
            sb.AppendLine("{");

            for (int i = 0; i < @enum.Members.Count; i++)
            {
                var info = semanticModel.GetDeclaredSymbol(@enum.Members[i])!;

                sb.AppendLine($"    public bool {info.Name} {{ get; set; }} ");
            }

            sb.AppendLine("}");

            context.AddSource(className, SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new ServicesReceiver());
    }
}
