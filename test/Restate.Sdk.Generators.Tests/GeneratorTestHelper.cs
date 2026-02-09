using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Restate.Sdk.Generators.Tests;

internal static class GeneratorTestHelper
{
    public static (GeneratorDriver Driver, Compilation OutputCompilation, ImmutableArray<Diagnostic> Diagnostics)
        RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = GetMetadataReferences();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new RestateClientGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out var diagnostics);

        return (driver, outputCompilation, diagnostics);
    }

    public static ImmutableArray<Diagnostic> GetGeneratorDiagnostics(GeneratorDriver driver)
    {
        return driver.GetRunResult().Diagnostics;
    }

    public static string? GetGeneratedSource(GeneratorDriver driver, string hintName)
    {
        var result = driver.GetRunResult();
        foreach (var generatedTree in result.GeneratedTrees)
            if (generatedTree.FilePath.EndsWith(hintName, StringComparison.Ordinal))
                return generatedTree.GetText().ToString();
        return null;
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Threading.Tasks.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(typeof(Context).Assembly.Location)
        ];
    }
}