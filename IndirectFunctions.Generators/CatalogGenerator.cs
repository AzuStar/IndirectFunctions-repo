using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace IndirectFunctions.Generators;

/// <summary>Builds statically rooted catalogs from the transformer's assembly bridges.</summary>
[Generator(LanguageNames.CSharp)]
public sealed class CatalogGenerator : IIncrementalGenerator
{
    private const string BridgeAttribute = "IndirectFunctions.NGeneratedBridgeAttribute";
    private const string CatalogAttribute = "IndirectFunctions.NGeneratedCatalogAttribute";
    private static readonly DiagnosticDescriptor InvalidBridge = new DiagnosticDescriptor("NIF200",
        "Invalid generated registration bridge", "Generated registration bridge '{0}' must expose an accessible static void Register() method",
        "IndirectFunctions", DiagnosticSeverity.Error, true);
    private static readonly DiagnosticDescriptor InvalidCatalog = new DiagnosticDescriptor("NIF201",
        "Invalid referenced catalog", "Referenced catalog '{0}' must expose a public static void Initialize() method",
        "IndirectFunctions", DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // The model contains only primitive values. Compilation symbols never enter generated output state.
        var model = context.CompilationProvider.Select(static (compilation, cancellationToken) => BuildModel(compilation))
            .WithComparer(CatalogModelComparer.Instance);
        context.RegisterSourceOutput(model, static (production, catalog) => Emit(production, catalog));
    }

    private static CatalogModel BuildModel(Compilation compilation)
    {
        if (compilation.GetTypeByMetadataName(CatalogAttribute) is null)
            return new CatalogModel(string.Empty, ImmutableArray<string>.Empty, ImmutableArray<string>.Empty,
                ImmutableArray<Problem>.Empty);

        var bridges = new SortedSet<string>(StringComparer.Ordinal);
        var dependencies = new SortedSet<string>(StringComparer.Ordinal);
        var problems = ImmutableArray.CreateBuilder<Problem>();
        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != BridgeAttribute || attribute.ConstructorArguments.Length != 1)
                continue;
            var bridge = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (bridge is null || !HasEntryPoint(bridge, "Register", false))
            {
                problems.Add(new Problem(true, bridge?.ToDisplayString() ?? "<missing>", attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
                continue;
            }
            bridges.Add(bridge.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols.OrderBy(item => item.Identity.ToString(), StringComparer.Ordinal))
        {
            foreach (var attribute in assembly.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != CatalogAttribute || attribute.ConstructorArguments.Length != 1
                    || attribute.ConstructorArguments[0].Value is not string name)
                    continue;
                var catalog = assembly.GetTypeByMetadataName(name);
                if (catalog is null || !HasEntryPoint(catalog, "Initialize", true))
                {
                    problems.Add(new Problem(false, name, null));
                    continue;
                }
                dependencies.Add(catalog.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }
        }
        return new CatalogModel(GetClassName(compilation.Assembly.Identity), bridges.ToImmutableArray(),
            dependencies.ToImmutableArray(), problems.ToImmutable());
    }

    private static bool HasEntryPoint(INamedTypeSymbol type, string methodName, bool publicOnly)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.Arity > 0 || current.IsFileLocal)
                return false;
            if (publicOnly && current.DeclaredAccessibility != Accessibility.Public)
                return false;
            if (!publicOnly && current.DeclaredAccessibility != Accessibility.Public && current.DeclaredAccessibility != Accessibility.Internal)
                return false;
        }
        return type.GetMembers(methodName).OfType<IMethodSymbol>().Any(method =>
            method.IsStatic && method.Arity == 0 && method.Parameters.Length == 0 && method.ReturnsVoid
            && (method.DeclaredAccessibility == Accessibility.Public || (!publicOnly && method.DeclaredAccessibility == Accessibility.Internal)));
    }

    private static string GetClassName(AssemblyIdentity identity)
    {
        var prefix = new string(identity.Name.Select(character => SyntaxFacts.IsIdentifierPartCharacter(character) ? character : '_').Take(32).ToArray());
        using (var hash = SHA256.Create())
        {
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(identity.ToString()));
            var suffix = BitConverter.ToString(bytes, 0, 16).Replace("-", string.Empty);
            return "Catalog_" + prefix + "_" + suffix;
        }
    }

    private static void Emit(SourceProductionContext context, CatalogModel model)
    {
        foreach (var problem in model.Problems)
            context.ReportDiagnostic(Diagnostic.Create(problem.Bridge ? InvalidBridge : InvalidCatalog,
                problem.Location ?? Location.None, problem.Name));
        if (model.ClassName.Length == 0 || (model.Bridges.IsEmpty && model.Dependencies.IsEmpty))
            return;

        var fullName = "IndirectFunctions.Generated." + model.ClassName;
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");
        source.AppendLine("#pragma warning disable CA2255, CS1591");
        source.Append("[assembly: global::IndirectFunctions.NGeneratedCatalogAttribute(\"").Append(fullName).AppendLine("\")]");
        source.AppendLine("namespace IndirectFunctions.Generated");
        source.AppendLine("{");
        source.Append("    public static class ").Append(model.ClassName).AppendLine();
        source.AppendLine("    {");
        source.AppendLine("        private static int __state;");
        source.AppendLine("        public static void Initialize()");
        source.AppendLine("        {");
        source.AppendLine("            var spin = new global::System.Threading.SpinWait();");
        source.AppendLine("            while (global::System.Threading.Volatile.Read(ref __state) != 2)");
        source.AppendLine("            {");
        source.AppendLine("                if (global::System.Threading.Interlocked.CompareExchange(ref __state, 1, 0) != 0)");
        source.AppendLine("                {");
        source.AppendLine("                    spin.SpinOnce();");
        source.AppendLine("                    continue;");
        source.AppendLine("                }");
        source.AppendLine("                try");
        source.AppendLine("                {");
        source.AppendLine("                    using (global::IndirectFunctions.NGenerated.BeginRegistrationBatch())");
        source.AppendLine("                    {");
        foreach (var dependency in model.Dependencies)
            source.Append("                        ").Append(dependency).AppendLine(".Initialize();");
        foreach (var bridge in model.Bridges)
            source.Append("                        ").Append(bridge).AppendLine(".Register();");
        source.AppendLine("                    }");
        source.AppendLine("                    global::System.Threading.Volatile.Write(ref __state, 2);");
        source.AppendLine("                    return;");
        source.AppendLine("                }");
        source.AppendLine("                catch");
        source.AppendLine("                {");
        source.AppendLine("                    global::System.Threading.Volatile.Write(ref __state, 0);");
        source.AppendLine("                    throw;");
        source.AppendLine("                }");
        source.AppendLine("            }");
        source.AppendLine("        }");
        source.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        source.AppendLine("        internal static void __InitializeModule() => Initialize();");
        source.AppendLine("    }");
        source.AppendLine("}");
        context.AddSource("IndirectFunctions.Catalog.g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private sealed class CatalogModel
    {
        internal CatalogModel(string className, ImmutableArray<string> bridges, ImmutableArray<string> dependencies, ImmutableArray<Problem> problems)
        {
            ClassName = className;
            Bridges = bridges;
            Dependencies = dependencies;
            Problems = problems;
        }
        internal string ClassName { get; }
        internal ImmutableArray<string> Bridges { get; }
        internal ImmutableArray<string> Dependencies { get; }
        internal ImmutableArray<Problem> Problems { get; }
    }

    private sealed class CatalogModelComparer : IEqualityComparer<CatalogModel>
    {
        internal static readonly CatalogModelComparer Instance = new CatalogModelComparer();
        public bool Equals(CatalogModel? left, CatalogModel? right) =>
            ReferenceEquals(left, right) || (left is not null && right is not null
                && left.Problems.IsEmpty && right.Problems.IsEmpty
                && left.ClassName == right.ClassName
                && left.Bridges.SequenceEqual(right.Bridges, StringComparer.Ordinal)
                && left.Dependencies.SequenceEqual(right.Dependencies, StringComparer.Ordinal));
        public int GetHashCode(CatalogModel model) => StringComparer.Ordinal.GetHashCode(model.ClassName);
    }

    private sealed class Problem
    {
        internal Problem(bool bridge, string name, Location? location)
        {
            Bridge = bridge;
            Name = name;
            Location = location;
        }
        internal bool Bridge { get; }
        internal string Name { get; }
        internal Location? Location { get; }
    }
}
