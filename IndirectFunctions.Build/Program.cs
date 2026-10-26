using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IndirectFunctions.Build;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var options = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < args.Length; i += 2)
            {
                if (i + 1 == args.Length || !args[i].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException("Expected pairs of --option value.");
                options[args[i][2..]] = args[i + 1];
            }
            string Required(string key) => options.TryGetValue(key, out var value) ? value : throw new ArgumentException($"Missing --{key}.");
            var sources = File.ReadAllLines(Required("sources")).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Path.GetFullPath).ToArray();
            var references = File.ReadAllLines(Required("references")).Where(File.Exists).Distinct(StringComparer.Ordinal).Select(p => MetadataReference.CreateFromFile(Path.GetFullPath(p))).ToArray();
            var languageText = options.GetValueOrDefault("langversion", "latest");
            if (!LanguageVersionFacts.TryParse(languageText, out var language)) throw new ArgumentException($"Invalid language version '{languageText}'.");
            var parse = new CSharpParseOptions(language, preprocessorSymbols: options.GetValueOrDefault("defines", "").Split([';', ','], StringSplitOptions.RemoveEmptyEntries));
            var trees = sources.Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), parse, p, Encoding.UTF8)).ToArray();
            var nullable = options.GetValueOrDefault("nullable", "disable") switch
            {
                "" or "disable" => NullableContextOptions.Disable,
                "warnings" => NullableContextOptions.Warnings,
                "annotations" => NullableContextOptions.Annotations,
                _ => NullableContextOptions.Enable
            };
            var outputKind = options.GetValueOrDefault("outputkind", "Library") switch { "Exe" => OutputKind.ConsoleApplication, "WinExe" => OutputKind.WindowsApplication, _ => OutputKind.DynamicallyLinkedLibrary };
            var compilation = CSharpCompilation.Create(options.GetValueOrDefault("assembly", "NIndirectConsumer"), trees, references,
                new CSharpCompilationOptions(outputKind, allowUnsafe: true, nullableContextOptions: nullable, checkOverflow: options.GetValueOrDefault("checked", "false").Equals("true", StringComparison.OrdinalIgnoreCase)));
            static string[] OptionalPaths(Dictionary<string, string> values, string key) => values.TryGetValue(key, out var list) && File.Exists(list) ? File.ReadAllLines(list).Where(p => !string.IsNullOrWhiteSpace(p)).Select(Path.GetFullPath).ToArray() : [];
            var pipeline = GeneratorPipeline.Run(compilation, OptionalPaths(options, "analyzers"), OptionalPaths(options, "analyzerconfig"), OptionalPaths(options, "additionalfiles"), parse, CancellationToken.None);
            foreach (var diagnostic in pipeline.Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)) Console.Error.WriteLine(diagnostic.ToString());
            if (pipeline.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return 1;
            compilation = pipeline.Compilation;
            // Transformation accepts valid C# only. Third-party generators already
            // enriched this original-source compilation, so retain every compiler
            // error before cell lifting can accidentally make rejected source legal.
            var sourceFailures = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (sourceFailures.Length != 0)
            {
                foreach (var failure in sourceFailures) Console.Error.WriteLine(failure.ToString());
                return 1;
            }
            var output = Path.GetFullPath(Required("output"));
            Directory.CreateDirectory(output);
            WriteIfChanged(Path.Combine(output, "retained-analyzers.txt"), string.Join(Environment.NewLine, pipeline.RetainedAnalyzerPaths) + Environment.NewLine);
            var transformer = new Transformer(compilation);
            var manifest = new List<string>();
            var bridges = new List<string>();
            foreach (var tree in trees)
            {
                var result = transformer.Transform(tree);
                if (result is null) { manifest.Add(tree.FilePath); continue; }
                string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tree.FilePath)))[..16];
                string destination = Path.Combine(output, $"{Path.GetFileNameWithoutExtension(tree.FilePath)}.{hash}.nindirect.cs");
                WriteIfChanged(destination, result.Source);
                manifest.Add(destination);
                bridges.AddRange(result.Bridges);
            }
            foreach (var generated in pipeline.GeneratedSyntaxTrees)
            {
                var model = compilation.GetSemanticModel(generated);
                foreach (var invocation in generated.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>())
                    if (model.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "Create", ContainingType.Name: "NFunc" or "NAction" or "ManagedNFunc" or "ManagedNAction", ContainingNamespace: var ns } && ns.ToDisplayString() == "IndirectFunctions")
                        throw new TransformException(invocation, "Indirect callback registrations must be declared in physical source, not generated by another source generator.");
                string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(generated.FilePath)))[..16];
                var destination = Path.Combine(output, "NIndirect.PreGenerated." + hash + ".g.cs");
                WriteIfChanged(destination, generated.GetRoot().ToFullString());
                manifest.Add(destination);
            }
            var metadata = Path.Combine(output, "NIndirect.Bridges.g.cs");
            WriteIfChanged(metadata, "// <auto-generated/>\n#nullable enable\n" + string.Join("\n", bridges.Select(b => $"[assembly: global::IndirectFunctions.NGeneratedBridgeAttribute(typeof(global::{b}))]")) + "\n");
            manifest.Add(metadata);
            WriteIfChanged(Path.Combine(output, "rewritten-sources.txt"), string.Join(Environment.NewLine, manifest) + Environment.NewLine);
            return 0;
        }
        catch (TransformException e)
        {
            var line = e.Node.GetLocation().GetLineSpan();
            Console.Error.WriteLine($"{line.Path}({line.StartLinePosition.Line + 1},{line.StartLinePosition.Character + 1}): error NIF100: {e.Message}");
            return 1;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"error NIF199: {e.Message}");
            return 1;
        }
    }

    private static void WriteIfChanged(string path, string content)
    {
        if (!File.Exists(path) || File.ReadAllText(path) != content) File.WriteAllText(path, content, new UTF8Encoding(false));
    }
}

internal sealed class TransformException(SyntaxNode node, string message) : Exception(message)
{
    public SyntaxNode Node { get; } = node;
}

internal sealed record Transformation(string Source, IReadOnlyList<string> Bridges);
