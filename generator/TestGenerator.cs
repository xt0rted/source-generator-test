namespace TestGenerator;

using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.CodeAnalysis;

[Generator]
public class TestGenerator : ISourceGenerator
{
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
        var sb = new StringBuilder();

        sb.AppendLine("namespace Directory.Generators {");
        sb.AppendLine("public class BoolsFromEnumFlags");
        sb.AppendLine("{");

        foreach (var item in Enum.GetValues(typeof(HomeCareProviderPersonalCareServices)))
        {
            sb.AppendLine($"public bool {item} {{ get; set; }} ");
        }

        sb.AppendLine("}");
        sb.AppendLine("}");

        context.AddSource("BoolsFromEnumFlagsNew2.g.cs", sb.ToString());
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();
    }
}

public enum HomeCareProviderPersonalCareServices
{
    Cleaning,
    Dressing,
    HelpWithPersonalHygiene,
}
