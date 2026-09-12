using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

using Truffleware.Abstractions.Messaging;
using Truffleware.CodeAnalysis.Generators;

namespace Truffleware.CodeAnalysis.Tests;

[TestClass]
[UsesVerify]
public sealed partial class RequestResponseGeneratorTests
{
    private const string AssemblyName = "Truffleware.RequestResponseGenerator.Tests";
    private const string RequestResponseInputFile = "RequestResponseGeneratorInput.cs";

    [TestMethod]
    public Task GeneratesHandlerSenderAndServiceRegistrations()
    {
        var driver = RunGeneratorFromInputFile(RequestResponseInputFile, out var outputCompilation);

        AssertNoErrors(outputCompilation);

        return Verify(driver);
    }

    private static GeneratorDriver RunGeneratorFromInputFile(string fileName, out Compilation outputCompilation)
    {
        var sourcePath = Path.Combine(GetInputsDirectory(), fileName);
        var sourceText = SourceText.From(File.ReadAllText(sourcePath));
        var compilation = CSharpCompilation.Create(
            assemblyName: AssemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(sourceText, path: sourcePath)],
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new RequestResponseGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out outputCompilation, out _);

        return driver;
    }

    private static void AssertNoErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(string.Empty, string.Join(Environment.NewLine, errors));
    }

    private static string GetInputsDirectory([CallerFilePath] string thisFilePath = "")
    {
        return Path.Combine(Path.GetDirectoryName(thisFilePath)!, "inputs");
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var assemblyLocations = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(static assembly => assembly.Location)
            .Append(typeof(RequestHandlerAttribute<,>).Assembly.Location)
            .Append(typeof(IServiceCollection).Assembly.Location)
            .Distinct()
            .ToArray();

        return assemblyLocations.Select(static path => MetadataReference.CreateFromFile(path));
    }
}
