namespace TestGenerator;

using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;

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

        var sb = new StringBuilder();

        sb.AppendLine("namespace Directory.Generators");
        sb.AppendLine("{");

        foreach (var @enum in receiver.EnumsToGenerate)
        {
            var semanticModel = context.Compilation.GetSemanticModel(@enum.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(@enum);

            var generateAttribute = symbol.GetAttributes().FirstOrDefault(at => at.AttributeClass?.Name == EnumExtensionsAttribute);
            if (generateAttribute is null)
            {
                continue;
            }

            sb.Append("    public class ");
            sb.Append(symbol.Name);
            sb.AppendLine("Flags");
            sb.AppendLine("    {");

            for (int i = 0; i < @enum.Members.Count; i++)
            {
                var info = semanticModel.GetDeclaredSymbol(@enum.Members[i])!;

                sb.AppendLine($"        public bool {info.Name} {{ get; set; }} ");
            }

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        context.AddSource("BoolsFromEnumFlagsNew2.g.cs", sb.ToString());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();
        context.RegisterForSyntaxNotifications(() => new ServicesReceiver());
    }
}
