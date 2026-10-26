using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace IndirectFunctions.Build;

internal sealed record GeneratorPipelineResult(
    CSharpCompilation Compilation,
    ImmutableArray<SyntaxTree> GeneratedSyntaxTrees,
    ImmutableArray<string> RetainedAnalyzerPaths,
    ImmutableArray<Diagnostic> Diagnostics);

/// <summary>
/// Run other generators against original physical source paths. Their generated trees become ordinary
/// compiler inputs after transformation, while the indirect callback catalog runs in final compilation.
/// </summary>
internal static class GeneratorPipeline
{
    private static readonly DiagnosticDescriptor LoadFailure = new("NIF201", "Generator assembly could not load",
        "Could not load analyzer/generator assembly '{0}': {1}", "IndirectFunctions", DiagnosticSeverity.Error, true);

    public static GeneratorPipelineResult Run(CSharpCompilation compilation, string[] analyzerPaths,
        string[] analyzerConfigPaths, string[] additionalFiles, CSharpParseOptions parse, CancellationToken token)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var configs = analyzerConfigPaths.Distinct(PathComparer).Select(path =>
            AnalyzerConfig.Parse(File.ReadAllText(path), Path.GetFullPath(path))).ToArray();
        var configSet = AnalyzerConfigSet.Create(configs, out var configDiagnostics);
        diagnostics.AddRange(configDiagnostics);
        var provider = new ConfigProvider(configSet);
        compilation = compilation.WithOptions(compilation.Options.WithSyntaxTreeOptionsProvider(new TreeConfigProvider(configSet)));
        var additionalTexts = additionalFiles.Distinct(PathComparer).Select(path =>
            (AdditionalText)new PhysicalAdditionalText(Path.GetFullPath(path))).ToImmutableArray();
        var paths = analyzerPaths.Select(Path.GetFullPath).Distinct(PathComparer).ToArray();
        var generators = ImmutableArray.CreateBuilder<ISourceGenerator>();
        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
        var retained = ImmutableArray.CreateBuilder<string>();
        using var loader = new DependencyLoader();
        foreach (var path in paths)
            loader.AddDependencyLocation(path);
        foreach (var path in paths)
        {
            token.ThrowIfCancellationRequested();
            var reference = new AnalyzerFileReference(path, loader);
            reference.AnalyzerLoadFailed += (_, failure) =>
            {
                if (failure.ErrorCode != AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers)
                    diagnostics.Add(Diagnostic.Create(LoadFailure, Location.None, path, failure.Message));
            };
            if (Path.GetFileName(path).Equals("IndirectFunctions.Generators.dll", StringComparison.OrdinalIgnoreCase))
            {
                // Validate marker syntax before it is rewritten. Keep this assembly for its catalog
                // generator in final csc; that generator must observe post-transform bridge metadata.
                retained.Add(path);
                analyzers.AddRange(reference.GetAnalyzers(LanguageNames.CSharp));
                continue;
            }
            var assemblyGenerators = reference.GetGenerators(LanguageNames.CSharp);
            if (assemblyGenerators.IsEmpty)
            {
                retained.Add(path);
                continue;
            }
            generators.AddRange(assemblyGenerators);
            // Assemblies combining analyzers and generators leave final csc entirely. Their analyzers
            // still see all original and generated declarations in this prepass.
            analyzers.AddRange(reference.GetAnalyzers(LanguageNames.CSharp));
        }

        ImmutableArray<SyntaxTree> generatedTrees = [];
        if (generators.Count != 0)
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generators.ToImmutable(), additionalTexts, parse, provider);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var generationDiagnostics, token);
            compilation = (CSharpCompilation)updated;
            generatedTrees = driver.GetRunResult().GeneratedTrees;
            diagnostics.AddRange(generationDiagnostics);
        }
        if (analyzers.Count != 0)
        {
            var options = new AnalyzerOptions(additionalTexts, provider);
            var analysis = compilation.WithAnalyzers(analyzers.ToImmutable(), options);
            diagnostics.AddRange(analysis.GetAnalyzerDiagnosticsAsync(token).GetAwaiter().GetResult());
        }
        return new(compilation, generatedTrees, retained.ToImmutable(), diagnostics.ToImmutable());
    }

    private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private sealed class PhysicalAdditionalText(string path) : AdditionalText
    {
        public override string Path { get; } = path;
        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SourceText.From(File.ReadAllText(Path), Encoding.UTF8);
        }
    }

    private sealed class ConfigOptions(ImmutableDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
    }

    private sealed class ConfigProvider(AnalyzerConfigSet configs) : AnalyzerConfigOptionsProvider
    {
        private readonly ConcurrentDictionary<string, AnalyzerConfigOptions> _files = new(PathComparer);
        public override AnalyzerConfigOptions GlobalOptions { get; } = new ConfigOptions(configs.GlobalConfigOptions.AnalyzerOptions);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Options(tree.FilePath);
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Options(textFile.Path);
        private AnalyzerConfigOptions Options(string path) => _files.GetOrAdd(path,
            file => new ConfigOptions(configs.GetOptionsForSourcePath(file).AnalyzerOptions));
    }

    private sealed class TreeConfigProvider(AnalyzerConfigSet configs) : SyntaxTreeOptionsProvider
    {
        public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var options = configs.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions;
            return options.TryGetValue("generated_code", out var text) && bool.TryParse(text, out var generated)
                ? generated ? GeneratedKind.MarkedGenerated : GeneratedKind.NotGenerated
                : GeneratedKind.Unknown;
        }
        public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return configs.GetOptionsForSourcePath(tree.FilePath).TreeOptions.TryGetValue(diagnosticId, out severity);
        }
        public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return configs.GlobalConfigOptions.TreeOptions.TryGetValue(diagnosticId, out severity);
        }
    }

    /// <summary>Share compiler API identities with the host, isolate package helper dependencies by analyzer directory.</summary>
    private sealed class DependencyLoader : IAnalyzerAssemblyLoader, IDisposable
    {
        private readonly Dictionary<string, List<string>> _dependencies = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Assembly> _loaded = new(PathComparer);
        private readonly List<PluginContext> _contexts = [];
        private readonly object _gate = new();

        public void AddDependencyLocation(string fullPath)
        {
            fullPath = Path.GetFullPath(fullPath);
            if (!File.Exists(fullPath))
                return;
            var name = AssemblyName.GetAssemblyName(fullPath).Name;
            if (name is null)
                return;
            if (!_dependencies.TryGetValue(name, out var paths))
                _dependencies.Add(name, paths = []);
            if (!paths.Contains(fullPath, PathComparer))
                paths.Add(fullPath);
        }

        public Assembly LoadFromPath(string fullPath)
        {
            fullPath = Path.GetFullPath(fullPath);
            lock (_gate)
            {
                if (_loaded.TryGetValue(fullPath, out var assembly))
                    return assembly;
                var context = new PluginContext(Path.GetDirectoryName(fullPath)!, _dependencies);
                _contexts.Add(context);
                assembly = context.LoadFromAssemblyPath(fullPath);
                _loaded.Add(fullPath, assembly);
                return assembly;
            }
        }

        public void Dispose()
        {
            foreach (var context in _contexts)
                context.Unload();
        }
    }

    private sealed class PluginContext(string directory, Dictionary<string, List<string>> dependencies) : AssemblyLoadContext(isCollectible: true)
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var name = assemblyName.Name;
            if (name is null)
                return null;
            if (name == typeof(Compilation).Assembly.GetName().Name)
                return typeof(Compilation).Assembly;
            if (name == typeof(CSharpCompilation).Assembly.GetName().Name)
                return typeof(CSharpCompilation).Assembly;
            if (name == typeof(ImmutableArray<>).Assembly.GetName().Name)
                return typeof(ImmutableArray<>).Assembly;
            var local = Path.Combine(directory, name + ".dll");
            if (File.Exists(local))
                return LoadFromAssemblyPath(local);
            if (dependencies.TryGetValue(name, out var candidates))
            {
                var match = candidates.FirstOrDefault(path => AssemblyName.GetAssemblyName(path).Version == assemblyName.Version) ?? candidates[0];
                return LoadFromAssemblyPath(match);
            }
            return null; // Runtime framework assemblies bind through the default context.
        }
    }
}
