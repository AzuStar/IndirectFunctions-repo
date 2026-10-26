using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace IndirectFunctions.Generators;

/// <summary>Validates source markers before the build transformer replaces them.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallbackAnalyzer : DiagnosticAnalyzer
{
    public const string KeyDiagnosticId = "NIF001";
    public const string CallbackDiagnosticId = "NIF002";
    public const string StateDiagnosticId = "NIF003";
    public const string GenericDiagnosticId = "NIF004";
    public const string PhysicalSourceDiagnosticId = "NIF005";
    public const string DuplicateDiagnosticId = "NIF006";
    public const string CollisionDiagnosticId = "NIF007";

    private static readonly DiagnosticDescriptor Key = Rule(KeyDiagnosticId,
        "Stable key required", "Callback key must be a nonblank compile-time constant string with valid Unicode");
    private static readonly DiagnosticDescriptor Callback = Rule(CallbackDiagnosticId,
        "Source callback required", "Callback must be a named method group or literal lambda; existing delegate values are unsupported");
    private static readonly DiagnosticDescriptor State = Rule(StateDiagnosticId,
        "Explicit state must use a static callback", "Explicit state must have a reference type and callback must be static; wrap mutable value state in NCell<T>");
    private static readonly DiagnosticDescriptor Generic = Rule(GenericDiagnosticId,
        "Closed registration required", "Callback registrations must use concrete closed types and must be outside generic type or method declarations");
    private static readonly DiagnosticDescriptor PhysicalSource = Rule(PhysicalSourceDiagnosticId,
        "Physical source required", "Callback creation must be in physical source transformed before compilation; generator-emitted callbacks are unsupported");
    private static readonly DiagnosticDescriptor Duplicate = Rule(DuplicateDiagnosticId,
        "Stable key reused at another creation site", "Stable key '{0}' already belongs to a callback creation site at {1}");
    private static readonly DiagnosticDescriptor Collision = Rule(CollisionDiagnosticId,
        "Stable code identity collision", "Stable keys '{0}' and '{1}' produce the same code identity");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Key, Callback, State, Generic, PhysicalSource, Duplicate, Collision);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            var registrations = new ConcurrentBag<Registration>();
            start.RegisterOperationAction(operation => Analyze(operation, registrations), OperationKind.Invocation);
            start.RegisterCompilationEndAction(end => CheckIdentities(end, registrations));
        });
    }

    private static void Analyze(OperationAnalysisContext context, ConcurrentBag<Registration> registrations)
    {
        var operation = (IInvocationOperation)context.Operation;
        if (!IsMarker(operation.TargetMethod) || operation.Syntax is not InvocationExpressionSyntax invocation)
            return;

        var keyArgument = operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "key");
        var callbackArgument = operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "callback");
        if (keyArgument is null || callbackArgument?.Syntax is not ArgumentSyntax callbackSyntax)
            return;

        var constant = keyArgument.Value.ConstantValue;
        if (!constant.HasValue || constant.Value is not string key || string.IsNullOrWhiteSpace(key))
        {
            context.ReportDiagnostic(Diagnostic.Create(Key, keyArgument.Syntax.GetLocation()));
            return;
        }
        try
        {
            new UTF8Encoding(false, true).GetByteCount(key);
        }
        catch (EncoderFallbackException)
        {
            context.ReportDiagnostic(Diagnostic.Create(Key, keyArgument.Syntax.GetLocation()));
            return;
        }

        var callbackExpression = Unwrap(callbackSyntax.Expression);
        var model = operation.SemanticModel!;
        var callbackSymbol = model.GetSymbolInfo(callbackExpression, context.CancellationToken).Symbol as IMethodSymbol;
        var lambda = callbackExpression as LambdaExpressionSyntax;
        if (lambda is null && callbackSymbol is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Callback, callbackExpression.GetLocation()));
            return;
        }

        var stateArgument = operation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "state");
        if (stateArgument is not null)
        {
            var staticCallback = lambda is not null
                ? lambda.Modifiers.Any(SyntaxKind.StaticKeyword)
                : callbackSymbol!.IsStatic;
            if (stateArgument.Value.Type?.IsReferenceType != true || !staticCallback)
                context.ReportDiagnostic(Diagnostic.Create(State, stateArgument.Syntax.GetLocation()));
        }

        var enclosingSymbol = model.GetEnclosingSymbol(invocation.SpanStart, context.CancellationToken);
        if (ContainsTypeParameter(operation.TargetMethod.ContainingType)
            || operation.TargetMethod.TypeArguments.Any(ContainsTypeParameter)
            || (stateArgument?.Value.Type is ITypeSymbol stateType && ContainsTypeParameter(stateType))
            || (callbackSymbol is not null && (ContainsTypeParameter(callbackSymbol.ContainingType)
                || callbackSymbol.TypeArguments.Any(ContainsTypeParameter)))
            || IsInGenericDeclaration(enclosingSymbol))
            context.ReportDiagnostic(Diagnostic.Create(Generic, invocation.GetLocation()));

        if (!IsPhysicalSource(invocation.SyntaxTree))
            context.ReportDiagnostic(Diagnostic.Create(PhysicalSource, invocation.GetLocation()));

        registrations.Add(new Registration(key, keyArgument.Syntax.GetLocation()));
    }

    private static void CheckIdentities(CompilationAnalysisContext context, ConcurrentBag<Registration> registrations)
    {
        var keys = new Dictionary<string, Registration>(StringComparer.Ordinal);
        var identities = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var registration in registrations.OrderBy(item => item.Location.SourceTree?.FilePath, StringComparer.Ordinal)
                     .ThenBy(item => item.Location.SourceSpan.Start))
        {
            if (keys.TryGetValue(registration.Key, out var previous))
            {
                var line = previous.Location.GetMappedLineSpan();
                var source = line.Path + ":" + (line.StartLinePosition.Line + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                context.ReportDiagnostic(Diagnostic.Create(Duplicate, registration.Location,
                    ImmutableArray.Create(previous.Location), null, registration.Key, source));
                continue;
            }
            keys.Add(registration.Key, registration);
            var identity = HashKey(registration.Key);
            if (identities.TryGetValue(identity, out var previousKey) && previousKey != registration.Key)
                context.ReportDiagnostic(Diagnostic.Create(Collision, registration.Location, registration.Key, previousKey));
            else
                identities.Add(identity, registration.Key);
        }
    }

    internal static bool IsMarker(IMethodSymbol method)
    {
        if (method.Name != "Create" || !method.IsStatic || method.ContainingNamespace.ToDisplayString() != "IndirectFunctions")
            return false;
        var name = method.ContainingType.Name;
        return name == "NFunc" || name == "NAction" || name == "ManagedNFunc" || name == "ManagedNAction";
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parentheses)
            expression = parentheses.Expression;
        return expression;
    }

    private static bool IsPhysicalSource(SyntaxTree tree)
    {
        var path = tree.FilePath;
        if (string.IsNullOrEmpty(path) || path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase))
            return false;
        var normalized = path.Replace('\\', '/');
        return normalized.IndexOf("/obj/", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static bool ContainsTypeParameter(ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
            return true;
        if (type is IArrayTypeSymbol array)
            return ContainsTypeParameter(array.ElementType);
        if (type is IPointerTypeSymbol pointer)
            return ContainsTypeParameter(pointer.PointedAtType);
        if (type is INamedTypeSymbol named)
            return named.TypeArguments.Any(ContainsTypeParameter)
                || (named.ContainingType is not null && ContainsTypeParameter(named.ContainingType));
        return false;
    }

    private static bool IsInGenericDeclaration(ISymbol? symbol)
    {
        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol is IMethodSymbol method && method.Arity > 0)
                return true;
            if (symbol is INamedTypeSymbol type && type.Arity > 0)
                return true;
        }
        return false;
    }

    private static string HashKey(string key)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes("IndirectFunctions.CodeId.v1\0" + key));
            return BitConverter.ToString(bytes, 0, 16);
        }
    }

    private static DiagnosticDescriptor Rule(string id, string title, string message) =>
        new DiagnosticDescriptor(id, title, message, "IndirectFunctions", DiagnosticSeverity.Error, true);

    private sealed class Registration
    {
        internal Registration(string key, Location location)
        {
            Key = key;
            Location = location;
        }
        internal string Key { get; }
        internal Location Location { get; }
    }
}
