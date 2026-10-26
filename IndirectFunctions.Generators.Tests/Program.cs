using System.Collections.Immutable;
using System.Reflection;
using IndirectFunctions.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

internal static class Program
{
    private static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path)).ToArray();
    private const string Runtime = """
        namespace IndirectFunctions
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class NGeneratedBridgeAttribute : System.Attribute
            { public NGeneratedBridgeAttribute(System.Type type) {} }
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class NGeneratedCatalogAttribute : System.Attribute
            { public NGeneratedCatalogAttribute(string name) {} }
            public static class NGenerated
            {
                private sealed class RegistrationBatch : System.IDisposable { public void Dispose() {} }
                public static System.IDisposable BeginRegistrationBatch() => new RegistrationBatch();
            }
            public sealed class NContext {}
            public sealed class NCell<T> { public T Value = default!; }
            public readonly struct NAction
            {
                public static NAction Create(NContext context, string key, System.Action callback) => default;
                public static NAction Create<TState>(NContext context, string key, TState state, System.Action<TState> callback) => default;
            }
            public readonly struct NFunc<TResult>
            {
                public static NFunc<TResult> Create(NContext context, string key, System.Func<TResult> callback) => default;
            }
            public readonly struct ManagedNAction
            {
                public static ManagedNAction Create(string key, System.Action callback) => default;
            }
        }
        """;
    private static int _assertions;

    public static async Task Main()
    {
        await ExpectDiagnostics("""
            class Callbacks
            {
                static void Method() {}
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    _ = IndirectFunctions.NAction.Create(context, "named.second", Method);
                    _ = IndirectFunctions.NAction.Create(context, "named", Method);
                    int value = 1;
                    _ = IndirectFunctions.NAction.Create(context, "captured", () => value++);
                    _ = IndirectFunctions.NAction.Create(context, "explicit", new IndirectFunctions.NCell<int>(), static state => state.Value++);
                    _ = IndirectFunctions.NAction.Create(callback: Method, key: "named.reordered", context: context);
                    _ = IndirectFunctions.ManagedNAction.Create("managed", () => value++);
                }
            }
            """);
        await ExpectDiagnostics("""
            class Callbacks
            {
                void Test(string dynamicKey)
                {
                    var context = new IndirectFunctions.NContext();
                    _ = IndirectFunctions.NAction.Create(context, dynamicKey, () => {});
                    _ = IndirectFunctions.NAction.Create(context, "", () => {});
                    _ = IndirectFunctions.NAction.Create(context, "   ", () => {});
                    _ = IndirectFunctions.NAction.Create(context, "\uD800", () => {});
                }
            }
            """, "NIF001", "NIF001", "NIF001", "NIF001");
        await ExpectDiagnostics("""
            class Callbacks
            {
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    System.Action callback = () => {};
                    _ = IndirectFunctions.NAction.Create(context, "delegate", callback);
                }
            }
            """, "NIF002");
        await ExpectDiagnostics("""
            class Callbacks
            {
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    int value = 1;
                    _ = IndirectFunctions.NAction.Create(context, "value", value, static state => {});
                    _ = IndirectFunctions.NAction.Create(context, "capture", new IndirectFunctions.NCell<int>(), state => value++);
                }
            }
            """, "NIF003", "NIF003");
        await ExpectDiagnostics("""
            class Callbacks
            {
                static void First() {}
                static void Second() {}
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    _ = IndirectFunctions.NAction.Create(context, "duplicate", First);
                    _ = IndirectFunctions.NAction.Create(context, "duplicate", Second);
                    _ = IndirectFunctions.NAction.Create(context, "lambda", () => {});
                    _ = IndirectFunctions.NAction.Create(context, "lambda", () => {});
                }
            }
            """, "NIF006", "NIF006");
        await ExpectDiagnostics("""
            class Callbacks
            {
                static void Method() {}
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    _ = IndirectFunctions.NAction.Create(context, "same.target", Method);
                    _ = IndirectFunctions.NAction.Create(context, "same.target", Method);
                }
            }
            """, "NIF006");
        await ExpectDiagnostics("""
            class Callbacks
            {
                static int Generic<T>() => 0;
                void Test()
                {
                    var context = new IndirectFunctions.NContext();
                    _ = IndirectFunctions.NFunc<int>.Create(context, "closed.generic", Generic<int>);
                    _ = IndirectFunctions.NFunc<int>.Create(context, "closed.generic", Generic<string>);
                }
            }
            """, "NIF006");
        await ExpectDiagnostics("""
            class Callbacks<T>
            {
                void Test()
                {
                    _ = IndirectFunctions.NAction.Create(new IndirectFunctions.NContext(), "generic", () => {});
                }
            }
            """, "NIF004");
        await ExpectDiagnostics("""
            class Callbacks
            {
                void Test()
                {
                    _ = IndirectFunctions.ManagedNAction.Create("generated", () => {});
                }
            }
            """, ["NIF005"], "/test/Producer.g.cs");

        TestCatalogGeneration();
        TestReferencedCatalog();
        TestInvalidBridge();
        TestInvalidReferencedCatalog();
        TestCatalogRetry();
        Console.WriteLine($"Generator/analyzer checks passed: {_assertions}.");
    }

    private static async Task ExpectDiagnostics(string source, params string[] expected) =>
        await ExpectDiagnostics(source, expected, "/test/Callbacks.cs");

    private static async Task ExpectDiagnostics(string source, string[] expected, string path)
    {
        var compilation = CreateCompilation(source, path: path);
        CheckNoErrors(compilation.GetDiagnostics());
        var diagnostics = await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new CallbackAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();
        var actual = diagnostics.Select(item => item.Id).Order().ToArray();
        Assert(actual.SequenceEqual(expected.Order()), $"Expected [{string.Join(", ", expected)}], got {string.Join(Environment.NewLine, diagnostics)}");
    }

    private static CSharpCompilation CreateCompilation(string source, string assemblyName = "AnalyzerFixture", string path = "/test/Callbacks.cs",
        IEnumerable<MetadataReference>? additionalReferences = null, bool runtime = true)
    {
        var trees = new List<SyntaxTree>();
        if (runtime)
            trees.Add(CSharpSyntaxTree.ParseText(Runtime, path: "/test/Runtime.cs"));
        trees.Add(CSharpSyntaxTree.ParseText(source, path: path));
        return CSharpCompilation.Create(assemblyName, trees, References.Concat(additionalReferences ?? []),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    private static (CSharpCompilation Compilation, GeneratorDriver Driver) Generate(CSharpCompilation compilation, GeneratorDriver? driver = null)
    {
        driver ??= CSharpGeneratorDriver.Create(new CatalogGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updated, out var diagnostics);
        CheckNoErrors(diagnostics);
        CheckNoErrors(updated.GetDiagnostics());
        return ((CSharpCompilation)updated, driver);
    }

    private static void TestCatalogGeneration()
    {
        var compilation = CreateCompilation("""
            [assembly: IndirectFunctions.NGeneratedBridge(typeof(BridgeZ))]
            [assembly: IndirectFunctions.NGeneratedBridge(typeof(BridgeA))]
            public static class BridgeZ { public static int Count; public static void Register() => Count++; }
            public static class BridgeA { public static int Count; public static void Register() => Count++; }
            """, "CatalogFixture");
        var result = Generate(compilation);
        var source = result.Driver.GetRunResult().GeneratedTrees.Single().ToString();
        Assert(source.IndexOf("global::BridgeA.Register();", StringComparison.Ordinal)
            < source.IndexOf("global::BridgeZ.Register();", StringComparison.Ordinal), "Bridge order is not deterministic.");
        var repeated = Generate(compilation, result.Driver).Driver.GetRunResult().GeneratedTrees.Single().ToString();
        Assert(source == repeated, "Repeated generation changed catalog.");
        var assembly = Load(result.Compilation);
        var catalog = assembly.GetTypes().Single(type => type.Name.StartsWith("Catalog_", StringComparison.Ordinal));
        var initialize = catalog.GetMethod("Initialize")!;
        Parallel.For(0, 100, _ => initialize.Invoke(null, null));
        Assert((int)assembly.GetType("BridgeA")!.GetField("Count")!.GetValue(null)! == 1, "Concurrent initialization registered bridge repeatedly.");
        Assert((int)assembly.GetType("BridgeZ")!.GetField("Count")!.GetValue(null)! == 1, "Module initializer did not register both bridges.");
    }

    private static void TestReferencedCatalog()
    {
        var dependency = Generate(CreateCompilation("""
            [assembly: IndirectFunctions.NGeneratedBridge(typeof(DependencyBridge))]
            public static class DependencyBridge { public static void Register() {} }
            """, "DependencyCatalog")).Compilation;
        using var bytes = new MemoryStream();
        var emitted = dependency.Emit(bytes);
        CheckNoErrors(emitted.Diagnostics);
        var reference = MetadataReference.CreateFromImage(bytes.ToArray());
        var consumer = CreateCompilation("public class Consumer {}", "ConsumerCatalog", additionalReferences: [reference], runtime: false);
        var result = Generate(consumer);
        var source = result.Driver.GetRunResult().GeneratedTrees.Single().ToString();
        Assert(source.Contains("global::IndirectFunctions.Generated.Catalog_DependencyCatalog_", StringComparison.Ordinal),
            "Consumer catalog failed to root dependency catalog directly.");
    }

    private static void TestInvalidBridge()
    {
        var compilation = CreateCompilation("""
            [assembly: IndirectFunctions.NGeneratedBridge(typeof(InvalidBridge))]
            public static class InvalidBridge { private static void Register() {} }
            """);
        var driver = CSharpGeneratorDriver.Create(new CatalogGenerator().AsSourceGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        Assert(diagnostics.Count(item => item.Id == "NIF200") == 1, "Invalid bridge did not report NIF200.");
    }

    private static void TestCatalogRetry()
    {
        // Keep generated catalog, remove automatic bootstrap so transient failure can be recovered deliberately.
        var result = Generate(CreateCompilation("""
            [assembly: IndirectFunctions.NGeneratedBridge(typeof(RetryBridge))]
            public static class RetryBridge
            {
                public static int Count;
                public static void Register() { if (++Count == 1) throw new System.InvalidOperationException("transient"); }
            }
            """, "RetryCatalog"));
        var generated = result.Driver.GetRunResult().GeneratedTrees.Single();
        var source = generated.ToString().Replace("[global::System.Runtime.CompilerServices.ModuleInitializer]", "", StringComparison.Ordinal);
        var compilation = result.Compilation.RemoveSyntaxTrees(generated).AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));
        var assembly = Load(compilation);
        var initialize = assembly.GetTypes().Single(type => type.Name.StartsWith("Catalog_", StringComparison.Ordinal)).GetMethod("Initialize")!;
        try
        {
            initialize.Invoke(null, null);
            throw new InvalidOperationException("Initialization should have failed.");
        }
        catch (TargetInvocationException exception) when (exception.InnerException?.Message == "transient") {}
        initialize.Invoke(null, null);
        initialize.Invoke(null, null);
        Assert((int)assembly.GetType("RetryBridge")!.GetField("Count")!.GetValue(null)! == 2, "Failed initialization did not reset state for retry.");
    }

    private static void TestInvalidReferencedCatalog()
    {
        var dependency = CreateCompilation("""
            [assembly: IndirectFunctions.NGeneratedCatalog("Missing.Catalog")]
            public class Dependency {}
            """, "InvalidDependencyCatalog");
        using var bytes = new MemoryStream();
        CheckNoErrors(dependency.Emit(bytes).Diagnostics);
        var consumer = CreateCompilation("public class Consumer {}", "InvalidConsumerCatalog",
            additionalReferences: [MetadataReference.CreateFromImage(bytes.ToArray())], runtime: false);
        CSharpGeneratorDriver.Create(new CatalogGenerator().AsSourceGenerator())
            .RunGeneratorsAndUpdateCompilation(consumer, out _, out var diagnostics);
        Assert(diagnostics.Count(item => item.Id == "NIF201") == 1, "Invalid catalog did not report NIF201.");
    }

    private static Assembly Load(CSharpCompilation compilation)
    {
        using var bytes = new MemoryStream();
        CheckNoErrors(compilation.Emit(bytes).Diagnostics);
        return Assembly.Load(bytes.ToArray());
    }

    private static void CheckNoErrors(IEnumerable<Diagnostic> diagnostics) =>
        Assert(!diagnostics.Any(item => item.Severity == DiagnosticSeverity.Error), string.Join(Environment.NewLine, diagnostics));

    private static void Assert(bool condition, string message)
    {
        _assertions++;
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
