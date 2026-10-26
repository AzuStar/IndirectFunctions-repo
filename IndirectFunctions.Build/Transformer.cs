using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace IndirectFunctions.Build;

internal sealed class Transformer(CSharpCompilation compilation)
{
    private readonly Dictionary<string, SyntaxNode> _keys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _codeIds = new(StringComparer.Ordinal);
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
    internal static string TypeName(ITypeSymbol type) => type.ToDisplayString(TypeFormat);
    internal static string Quote(string value) => SymbolDisplay.FormatLiteral(value, true);
    internal static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..16];

    public Transformation? Transform(SyntaxTree tree)
    {
        var model = compilation.GetSemanticModel(tree, true);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        var callbacks = new List<Callback>();
        foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method || method.Name != "Create" || method.ContainingNamespace.ToDisplayString() != "IndirectFunctions") continue;
            var holder = method.ContainingType;
            if (holder.Name is not ("NFunc" or "NAction" or "ManagedNFunc" or "ManagedNAction")) continue;
            bool managed = holder.Name.StartsWith("Managed", StringComparison.Ordinal);
            int keyIndex = managed ? 0 : 1;
            if (invocation.ArgumentList.Arguments.Count < keyIndex + 2) throw new TransformException(invocation, "Malformed callback creation marker.");
            var orderedArguments = OrderArguments(invocation, method);
            var keyArgument = orderedArguments[keyIndex];
            var keyConstant = model.GetConstantValue(keyArgument);
            if (!keyConstant.HasValue || keyConstant.Value is not string key || string.IsNullOrWhiteSpace(key)) throw new TransformException(keyArgument, "Callback key must be a nonempty compile-time constant string.");
            byte[] keyBytes;
            try { keyBytes = new UTF8Encoding(false, true).GetBytes(key); }
            catch (EncoderFallbackException) { throw new TransformException(keyArgument, "Callback key must contain valid Unicode for strict UTF-8 encoding."); }
            var domain = Encoding.ASCII.GetBytes("IndirectFunctions.CodeId.v1\0");
            var codeInput = new byte[domain.Length + keyBytes.Length];
            domain.CopyTo(codeInput, 0);
            keyBytes.CopyTo(codeInput, domain.Length);
            var codeId = Convert.ToHexString(SHA256.HashData(codeInput).AsSpan(0, 16));
            if (_codeIds.TryGetValue(codeId, out var collision) && collision != key) throw new TransformException(keyArgument, $"128-bit callback code ID collision between keys '{collision}' and '{key}'.");
            _codeIds[codeId] = key;
            if (_keys.TryGetValue(key, out var previous)) throw new TransformException(invocation, $"Duplicate callback key '{key}' (first declaration: {previous.GetLocation().GetLineSpan()}).");
            _keys.Add(key, invocation);
            if (holder.TypeArguments.Any(ContainsTypeParameter)) throw new TransformException(invocation, "Open generic callback registrations are unsupported; use a concrete closed signature and unique key.");
            var body = orderedArguments[^1];
            var state = orderedArguments.Length == keyIndex + 3 ? orderedArguments[keyIndex + 1] : null;
            var owner = invocation.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (invocation.Ancestors().OfType<TypeDeclarationSyntax>().Any(t => t.TypeParameterList is not null) || invocation.Ancestors().OfType<MethodDeclarationSyntax>().Any(m => m.TypeParameterList is not null) || invocation.Ancestors().OfType<LocalFunctionStatementSyntax>().Any(m => m.TypeParameterList is not null)) throw new TransformException(invocation, "Callback creation inside an open generic type or method is unsupported.");
            var callback = new Callback(invocation, body, key, holder, managed, state, owner, model, managed ? null : orderedArguments[0]);
            callback.Analyze();
            callbacks.Add(callback);
        }
        if (callbacks.Count == 0) return null;
        var cells = new Dictionary<ISymbol, Cell>(SymbolEqualityComparer.Default);
        foreach (var callback in callbacks)
            foreach (var symbol in callback.Captured)
                if (!cells.ContainsKey(symbol)) cells.Add(symbol, new Cell(symbol, model));
        foreach (var callback in callbacks) callback.SetCells(cells);
        var bridge = "__NIndirectBridge_" + Hash(tree.FilePath);
        var rewriter = new SourceRewriter(model, callbacks, cells, bridge);
        var transformed = (CompilationUnitSyntax)rewriter.Visit(root)!;
        var members = rewriter.BuildTopLevelMembers();
        transformed = transformed.AddMembers(members.ToArray());
        // Retain original trivia and token order. Each newly expanded declaration has a
        // line directive; this keeps diagnostics/caller line info anchored to user source.
        string nullable = compilation.Options.NullableContextOptions switch
        {
            NullableContextOptions.Enable => "#nullable enable\n",
            NullableContextOptions.Disable => "#nullable disable\n",
            NullableContextOptions.Annotations => "#nullable enable annotations\n#nullable disable warnings\n",
            _ => "#nullable disable annotations\n#nullable enable warnings\n"
        };
        string source = "// <auto-generated/>\n" + nullable + "#line 1 " + Quote(tree.FilePath) + "\n" + transformed.ToFullString();
        string prefix = root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() is { } ns ? ns + "." : "";
        return new Transformation(source, [prefix + bridge]);
    }

    private static bool ContainsTypeParameter(ITypeSymbol type) => type is ITypeParameterSymbol || type is INamedTypeSymbol named && named.TypeArguments.Any(ContainsTypeParameter) || type is IArrayTypeSymbol array && ContainsTypeParameter(array.ElementType);

    private static ExpressionSyntax[] OrderArguments(InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        var result = new ExpressionSyntax[method.Parameters.Length];
        for (int i = 0; i < invocation.ArgumentList.Arguments.Count; ++i)
        {
            var argument = invocation.ArgumentList.Arguments[i];
            int ordinal = argument.NameColon is null ? i : method.Parameters.First(p => p.Name == argument.NameColon.Name.Identifier.ValueText).Ordinal;
            result[ordinal] = argument.Expression;
        }
        if (result.Any(x => x is null)) throw new TransformException(invocation, "Creation marker requires every argument.");
        return result;
    }

    internal sealed class Cell
    {
        public ISymbol Symbol { get; }
        public string Name { get; }
        public string Type { get; }
        public SyntaxNode Declaration { get; }
        public Cell(ISymbol symbol, SemanticModel model)
        {
            Symbol = symbol;
            Declaration = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() ?? symbol.ContainingSymbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() ?? model.SyntaxTree.GetRoot();
            Name = "__ncell_" + symbol.Name.TrimStart('@') + "_" + Declaration.SpanStart;
            var type = symbol switch { ILocalSymbol local => local.Type, IParameterSymbol parameter => parameter.Type, _ => throw new TransformException(Declaration, "Unsupported captured symbol.") };
            if (type.IsRefLikeType || type.TypeKind == TypeKind.Pointer || symbol is ILocalSymbol { RefKind: not RefKind.None } || symbol is IParameterSymbol { RefKind: not RefKind.None }) throw new TransformException(Declaration, "Ref, pointer, and ref-like captures cannot be serialized into a capture cell.");
            Type = TypeName(type);
            if (Declaration is not (VariableDeclaratorSyntax or ParameterSyntax or ForEachStatementSyntax or SingleVariableDesignationSyntax or CatchDeclarationSyntax or AccessorDeclarationSyntax or CompilationUnitSyntax)) throw new TransformException(Declaration, "Unsupported capture declaration.");
        }
        public string CellType => $"global::IndirectFunctions.NCell<{Type}>";
        public string Access => Name + ".Value";
        public string RawName => Name + "_pattern";
        public bool AllocateAtScopeEntry => Declaration is VariableDeclaratorSyntax { Parent.Parent: LocalDeclarationStatementSyntax { UsingKeyword.RawKind: 0 } };
    }

    internal sealed class Callback
    {
        public InvocationExpressionSyntax Invocation { get; }
        public ExpressionSyntax Body { get; }
        public string Key { get; }
        public INamedTypeSymbol Holder { get; }
        public bool Managed { get; }
        public ExpressionSyntax? State { get; }
        public TypeDeclarationSyntax? Owner { get; }
        public SemanticModel Model { get; }
        public ExpressionSyntax? Context { get; }
        public HashSet<ISymbol> Captured { get; } = new(SymbolEqualityComparer.Default);
        public List<IMethodSymbol> LocalFunctions { get; } = [];
        public Dictionary<ISymbol, Cell> Cells { get; private set; } = new(SymbolEqualityComparer.Default);
        public IMethodSymbol? TargetMethod { get; private set; }
        public bool NeedsThis { get; private set; }
        public ExpressionSyntax? Receiver { get; private set; }
        public bool BaseReceiver { get; private set; }
        public string Name => "__ncallback_" + Hash(Key);
        public string EnvName => Name + "_State";
        public string ThunkName => Name + "_Thunk";
        public string InitName => Name + "_Register";
        public bool IsAction => Holder.Name.EndsWith("NAction", StringComparison.Ordinal);
        public string ReturnType => IsAction ? "void" : TypeName(Holder.TypeArguments[^1]);
        public ITypeSymbol[] Arguments => (IsAction ? Holder.TypeArguments : Holder.TypeArguments.Take(Holder.TypeArguments.Length - 1)).ToArray();
        public string RuntimeHolder => "global::IndirectFunctions." + (IsAction ? "NAction" : "NFunc") + (Holder.TypeArguments.Length == 0 ? "" : "<" + string.Join(",", Holder.TypeArguments.Select(TypeName)) + ">");
        public string ProfileHolder => Managed ? "global::IndirectFunctions.Managed" + (IsAction ? "NAction" : "NFunc") + (Holder.TypeArguments.Length == 0 ? "" : "<" + string.Join(",", Holder.TypeArguments.Select(TypeName)) + ">") : RuntimeHolder;
        public string? StateType => State is null ? null : TypeName(Model.GetTypeInfo(State).Type ?? throw new TransformException(State, "Explicit state type could not be resolved."));
        private ITypeSymbol? ReceiverSymbolType => Receiver is null ? null : Receiver is ThisExpressionSyntax ? (ITypeSymbol)Model.GetDeclaredSymbol(Owner!)! : TargetMethod!.ReducedFrom is not null ? Model.GetTypeInfo(Receiver).Type! : TargetMethod.ContainingType;
        public string? ReceiverType => ReceiverSymbolType is null ? null : TypeName(ReceiverSymbolType);
        public bool HasEnvironment => Captured.Count != 0 || NeedsThis || State is not null || Receiver is not null;
        public bool BaseLambda => Body.DescendantNodesAndSelf().Any(n => n is BaseExpressionSyntax) && Body is AnonymousFunctionExpressionSyntax || LocalFunctions.Any(f => f.DeclaringSyntaxReferences.First().GetSyntax().DescendantNodes().Any(n => n is BaseExpressionSyntax));

        public Callback(InvocationExpressionSyntax invocation, ExpressionSyntax body, string key, INamedTypeSymbol holder, bool managed, ExpressionSyntax? state, TypeDeclarationSyntax? owner, SemanticModel model, ExpressionSyntax? context)
        { Invocation = invocation; Body = body; Key = key; Holder = holder; Managed = managed; State = state; Owner = owner; Model = model; Context = context; }

        public void Analyze()
        {
            if (State is not null && Model.GetTypeInfo(State).Type is not { IsReferenceType: true }) throw new TransformException(State, "Explicit state must be a reference type; wrap mutable value state in NCell<T>.");
            if (Body is LambdaExpressionSyntax or AnonymousMethodExpressionSyntax)
            {
                AnalyzeBody(Body);
                if (State is not null && (Captured.Count != 0 || Body is LambdaExpressionSyntax { Modifiers: var modifiers } && !modifiers.Any(SyntaxKind.StaticKeyword))) throw new TransformException(Body, "Explicit-state callback must be static and cannot capture lexical variables.");
                return;
            }
            if (Model.GetSymbolInfo(Body).Symbol is not IMethodSymbol method) throw new TransformException(Body, "Callback must be a literal lambda or named method group; delegate variables are unsupported.");
            TargetMethod = method;
            if (State is not null && !method.IsStatic) throw new TransformException(Body, "Explicit-state callback method must be static.");
            if (method.MethodKind == MethodKind.LocalFunction) { AddLocalFunction(method); return; }
            if (!method.IsStatic || method.ReducedFrom is not null)
            {
                Receiver = Body is MemberAccessExpressionSyntax access ? access.Expression : ThisExpression();
                if (Receiver is BaseExpressionSyntax) { BaseReceiver = true; Receiver = ThisExpression(); }
            }
        }

        private void AnalyzeBody(SyntaxNode node)
        {
            var flow = Model.AnalyzeDataFlow(node);
            if (flow is { Succeeded: true })
                foreach (var symbol in flow.CapturedInside)
                {
                    if (symbol is not (ILocalSymbol or IParameterSymbol)) continue;
                    if (symbol is ILocalSymbol { IsConst: true }) continue;
                    var declaration = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                    if (symbol is IParameterSymbol { IsThis: true }) { NeedsThis = true; continue; }
                    if (declaration is null && symbol is IParameterSymbol || declaration is not null && !node.Span.Contains(declaration.Span)) Captured.Add(symbol);
                }
            foreach (var identifier in node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            {
                if (identifier.Ancestors().OfType<InvocationExpressionSyntax>().Any(i => i.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" })) continue;
                var symbol = Model.GetSymbolInfo(identifier).Symbol;
                if (symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } local) AddLocalFunction(local);
                if (symbol is IFieldSymbol { IsStatic: false } or IPropertySymbol { IsStatic: false } or IEventSymbol { IsStatic: false } || symbol is IMethodSymbol { IsStatic: false, MethodKind: not MethodKind.LocalFunction } && identifier.Parent is not MemberAccessExpressionSyntax)
                {
                    if (identifier.Parent is not MemberAccessExpressionSyntax { Name: var name } || name != identifier) NeedsThis = true;
                }
            }
            if (node.DescendantNodesAndSelf().Any(n => n is ThisExpressionSyntax or BaseExpressionSyntax && !n.Ancestors().OfType<InvocationExpressionSyntax>().Any(i => i.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" }))) NeedsThis = true;
        }

        private void AddLocalFunction(IMethodSymbol local)
        {
            local = local.OriginalDefinition;
            var syntax = local.DeclaringSyntaxReferences.First().GetSyntax();
            // Functions declared inside the transplanted callback remain in that
            // callback's lexical scope. Only references crossing its boundary lift.
            if (Body.Span.Contains(syntax.Span)) return;
            if (LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, local))) return;
            LocalFunctions.Add(local);
            AnalyzeBody(syntax);
        }

        public void SetCells(Dictionary<ISymbol, Cell> cells) => Cells = Captured.ToDictionary(s => s, s => cells[s], SymbolEqualityComparer.Default);
        public string SchemaKey => "nindirect.capture.v1:" + Key + ":" + string.Join(";", Cells.Values.OrderBy(c => c.Declaration.SpanStart).Select(c => c.Symbol.Name + ":" + c.Type)) + (NeedsThis ? ";this:" + Model.GetDeclaredSymbol(Owner!)!.ToDisplayString() : "") + (StateType is null ? "" : ";state:" + StateType) + (ReceiverType is null ? "" : ";receiver:" + ReceiverType);
        public string SchemaExpression => EnvName + ".Id";
        public string CodeExpression => Name + "_CodeId";
        public string CellField(Cell cell) => "__" + cell.Name;

        public string EnvironmentExpression(Func<SyntaxNode, SyntaxNode?> visit, Func<Cell,string>? cellReference = null, string? qualification = null)
        {
            if (!HasEnvironment) return "null";
            var fields = Cells.Values.Select(c => CellField(c) + "=" + (cellReference?.Invoke(c) ?? c.Name)).ToList();
            if (NeedsThis) fields.Add("__this=this");
            if (State is not null) fields.Add("__state=" + visit(State)!.ToFullString());
            if (Receiver is not null)
            {
                var receiver = Receiver is ThisExpressionSyntax ? "this" : visit(Receiver)!.ToFullString();
                var type = ReceiverSymbolType!;
                // Method-group conversion checks null at construction, unlike a delayed call.
                if (type.IsReferenceType) receiver = "(" + receiver + " ?? throw new global::System.NullReferenceException(\"Method-group receiver is null.\"))";
                fields.Add("__target=" + receiver);
            }
            return "new " + qualification + EnvName + " {" + string.Join(",", fields) + "}";
        }

        public string CreateExpression(Func<SyntaxNode, SyntaxNode?> visit, Func<Cell,string>? cellReference = null, string? qualification = null)
        {
            var environment = EnvironmentExpression(visit, cellReference, qualification);
            var arguments = new List<string>();
            foreach (var original in Invocation.ArgumentList.Arguments)
            {
                var expression = original.Expression;
                if (expression == Context) arguments.Add("context:" + visit(expression)!.ToFullString());
                else if (expression == State || State is null && expression == Body) arguments.Add("capture:" + environment);
                else if (expression != Body) arguments.Add("id:" + qualification + CodeExpression);
            }
            return ProfileHolder + ".CreateRegistered(" + string.Join(",", arguments) + ")";
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateMembers(Dictionary<InvocationExpressionSyntax, Callback> callbacks)
        {
            yield return ParseMemberDeclaration($"internal static readonly global::IndirectFunctions.NCodeId {CodeExpression}=global::IndirectFunctions.NCodeId.FromKey({Quote(Key)});")!;
            var fields = new List<(string Type, string Name, string PersistedName)>();
            fields.AddRange(Cells.Values.OrderBy(c => c.Declaration.SpanStart).Select((c, index) => (c.CellType, CellField(c), "capture:" + index + ":" + c.Symbol.Name)));
            if (NeedsThis) fields.Add((TypeName((INamedTypeSymbol)Model.GetDeclaredSymbol(Owner!)!), "__this", "this"));
            if (StateType is not null) fields.Add((StateType, "__state", "state"));
            if (ReceiverType is not null) fields.Add((ReceiverType, "__target", "target"));
            if (HasEnvironment)
            {
                string env = $"private sealed class {EnvName} : global::IndirectFunctions.INCaptureState {{" +
                    string.Join("", fields.Select(f => $"public {f.Type} {f.Name}=default!;")) +
                    $"public static readonly global::IndirectFunctions.NCodeId Id = global::IndirectFunctions.NCodeId.FromKey({Quote(SchemaKey)}); public global::IndirectFunctions.NCodeId CaptureSchemaId => Id;" +
                    "public void Write(global::IndirectFunctions.NCaptureWriter writer){" + string.Join("", fields.Select(f => $"writer.Field({Quote(f.PersistedName)}, {f.Name});")) + "}" +
                    "public void Read(global::IndirectFunctions.NCaptureReader reader){" + string.Join("", fields.Select(f => $"{f.Name}=reader.Field<{f.Type}>({Quote(f.PersistedName)});")) + "}}";
                yield return ParseMemberDeclaration(env)!;
            }
            var methodBody = MakeThunkBody(callbacks, out bool isAsync);
            bool? checkedContext = Invocation.Ancestors().Select(n => n switch { CheckedStatementSyntax statement => (bool?)statement.IsKind(SyntaxKind.CheckedStatement), CheckedExpressionSyntax expression => expression.IsKind(SyntaxKind.CheckedExpression), _ => null }).FirstOrDefault(x => x.HasValue);
            if (checkedContext.HasValue) methodBody = "{" + (checkedContext.Value ? "checked" : "unchecked") + methodBody + "}";
            string parameters = string.Join(",", Arguments.Select((t, i) => TypeName(t) + " __arg" + i));
            if (BaseLambda)
            {
                yield return ParseMemberDeclaration($"private {(isAsync ? "async " : "")}{ReturnType} {Name}_Instance(object? __capture{(parameters.Length == 0 ? "" : "," + parameters)}) {methodBody}")!;
                string invocation = $"(({EnvName})__capture!).__this.{Name}_Instance(__capture{string.Concat(Arguments.Select((_, i) => ",__arg" + i))})";
                methodBody = "{" + (IsAction ? "" : "return ") + invocation + ";}";
                isAsync = false;
            }
            string thunk = $"private static {(isAsync ? "async " : "")}{ReturnType} {ThunkName}(object? __capture{(parameters.Length == 0 ? "" : "," + parameters)}) {methodBody}";
            yield return ParseMemberDeclaration(thunk)!;
            if (BaseReceiver)
            {
                string methodName = ((MemberAccessExpressionSyntax)Body).Name.ToFullString();
                string call = $"base.{methodName}({string.Join(",", Arguments.Select((_, i) => "__arg" + i))})";
                yield return ParseMemberDeclaration($"private {ReturnType} {Name}_Base({parameters}){{{(IsAction ? "" : "return ")}{call};}}")!;
            }
            foreach (var local in LocalFunctions)
            {
                var syntax = (LocalFunctionStatementSyntax)local.DeclaringSyntaxReferences.First().GetSyntax();
                var rewrite = new ThunkRewriter(this, callbacks, BaseLambda);
                var body = syntax.Body is not null ? rewrite.Visit(syntax.Body)!.ToFullString() : "{" + (local.ReturnsVoid ? "" : "return ") + rewrite.Visit(syntax.ExpressionBody!.Expression)!.ToFullString() + ";}";
                var parameterList = string.Join(",", local.Parameters.Select(p => (p.RefKind == RefKind.None ? "" : p.RefKind.ToString().ToLowerInvariant() + " ") + TypeName(p.Type) + " " + p.Name));
                string typeParameters = syntax.TypeParameterList?.ToFullString() ?? "";
                string constraints = string.Join(" ", syntax.ConstraintClauses.Select(c => c.ToFullString()));
                string lifted = $"private {(BaseLambda ? "" : "static ")}{(local.IsAsync ? "async " : "")}{TypeName(local.ReturnType)} {Name}_Local_{syntax.SpanStart}{typeParameters}({(HasEnvironment ? EnvName : "object?")} __env{(parameterList.Length == 0 ? "" : "," + parameterList)}) {constraints} {body}";
                yield return ParseMemberDeclaration(lifted)!;
            }
            string registration = "";
            if (HasEnvironment)
            {
                registration += $"global::IndirectFunctions.NGenerated.RegisterCaptureSchema(new global::IndirectFunctions.NCaptureSchema({Quote(SchemaKey)}, static()=>new {EnvName}()));";
                var knownCells = Cells.Values.Select(c => c.CellType).ToHashSet(StringComparer.Ordinal);
                foreach (var cell in Cells.Values) foreach (var type in KnownCellTypes(cell.Symbol is ILocalSymbol local ? local.Type : ((IParameterSymbol)cell.Symbol).Type)) knownCells.Add(type);
                if (State is not null) foreach (var type in KnownCellTypes(Model.GetTypeInfo(State).Type!)) knownCells.Add(type);
                if (ReceiverSymbolType is not null) foreach (var type in KnownCellTypes(ReceiverSymbolType)) knownCells.Add(type);
                registration += string.Join("", knownCells.Order(StringComparer.Ordinal).Select(t => $"_ = {t}.SchemaId;"));
            }
            registration += $"{RuntimeHolder}.RegisterGenerated({Quote(Key)}, &{ThunkName}, {(HasEnvironment ? SchemaExpression : "null")});";
            yield return ParseMemberDeclaration($"private static class {InitName} {{ public static unsafe void Register(){{{registration}}} }}")!;
        }

        private static IEnumerable<string> KnownCellTypes(ITypeSymbol type)
        {
            while (type is INamedTypeSymbol { Name: "NCell", Arity: 1 } named && named.ContainingNamespace.ToDisplayString() == "IndirectFunctions")
            {
                yield return TypeName(named);
                type = named.TypeArguments[0];
            }
        }

        private string MakeThunkBody(Dictionary<InvocationExpressionSyntax, Callback> callbacks, out bool isAsync)
        {
            var rewrite = new ThunkRewriter(this, callbacks, BaseLambda);
            string prefix = HasEnvironment ? $"var __env=({EnvName})__capture!;" : "";
            isAsync = false;
            if (Body is LambdaExpressionSyntax lambda)
            {
                isAsync = lambda.AsyncKeyword.RawKind != 0;
                var parameters = lambda switch { SimpleLambdaExpressionSyntax simple => [simple.Parameter], ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters.ToArray(), _ => Array.Empty<ParameterSyntax>() };
                for (int i = 0; i < parameters.Length; ++i)
                {
                    var parameter = Model.GetDeclaredSymbol(parameters[i])!;
                    rewrite.Parameters[parameter] = State is not null && i == 0 ? "__env.__state" : "__arg" + (State is null ? i : i - 1);
                }
                if (lambda.Body is BlockSyntax block) return "{" + prefix + string.Join("", rewrite.RewriteBodyParameters(lambda, (BlockSyntax)rewrite.Visit(block)!).Statements.Select(s => s.ToFullString())) + "}";
                string expression = rewrite.Visit(lambda.Body)!.ToFullString();
                bool returnsVoid = IsAction || isAsync && Holder.TypeArguments[^1] is INamedTypeSymbol { Name: "Task", Arity: 0 };
                var expressionBlock = Block(ParseStatement((returnsVoid ? "" : "return ") + expression + ";"));
                return "{" + prefix + string.Join("", rewrite.RewriteBodyParameters(lambda, expressionBlock).Statements.Select(s => s.ToFullString())) + "}";
            }
            if (Body is AnonymousMethodExpressionSyntax anonymous)
            {
                isAsync = anonymous.AsyncKeyword.RawKind != 0;
                var parameters = anonymous.ParameterList?.Parameters ?? default;
                for (int i = 0; i < parameters.Count; ++i) rewrite.Parameters[Model.GetDeclaredSymbol(parameters[i])!] = State is not null && i == 0 ? "__env.__state" : "__arg" + (State is null ? i : i - 1);
                return "{" + prefix + string.Join("", rewrite.RewriteBodyParameters(anonymous, (BlockSyntax)rewrite.Visit(anonymous.Block)!).Statements.Select(s => s.ToFullString())) + "}";
            }
            var args = string.Join(",", Arguments.Select((_, i) => "__arg" + i));
            string target;
            if (TargetMethod!.MethodKind == MethodKind.LocalFunction)
            {
                var local = TargetMethod.DeclaringSyntaxReferences.First().GetSyntax();
                string typeArguments = TargetMethod.IsGenericMethod ? "<" + string.Join(",", TargetMethod.TypeArguments.Select(TypeName)) + ">" : "";
                target = Name + "_Local_" + local.SpanStart + typeArguments + "(" + (HasEnvironment ? "__env" : "null") + (args.Length == 0 ? "" : "," + args) + ")";
            }
            else
            {
                string methodName = Body is MemberAccessExpressionSyntax access ? access.Name.ToFullString() : Body.ToFullString();
                if (Receiver is not null) target = "__env.__target." + (BaseReceiver ? Name + "_Base" : methodName) + "(" + (State is null ? "" : "__env.__state" + (args.Length == 0 ? "" : ",")) + args + ")";
                else target = Body.ToFullString() + "(" + (State is null ? "" : "__env.__state" + (args.Length == 0 ? "" : ",")) + args + ")";
            }
            return "{" + prefix + (IsAction ? "" : "return ") + target + ";}";
        }
    }

    private abstract class CellRewriter(SemanticModel model, Dictionary<ISymbol, Cell> cells) : CSharpSyntaxRewriter
    {
        protected SemanticModel Model { get; } = model;
        protected Dictionary<ISymbol, Cell> AllCells { get; } = cells;
        protected virtual string ReadCell(Cell cell) => cell.Access;
        protected virtual string CellReference(Cell cell) => cell.Name;
        protected virtual string ParameterInitial(ISymbol symbol) => symbol.Name;
        private static ReturnStatementSyntax ReturnValue(ExpressionSyntax expression) => ReturnStatement(expression).WithReturnKeyword(Token(SyntaxKind.ReturnKeyword).WithTrailingTrivia(Space));
        protected ExpressionSyntax ReadCellExpression(Cell cell, IdentifierNameSyntax original)
        {
            string expression = ReadCell(cell);
            bool variableTarget = original.Parent is AssignmentExpressionSyntax assignment && assignment.Left == original || original.Parent is ArgumentSyntax { RefKindKeyword.RawKind: not 0 };
            if (!variableTarget && Model.GetTypeInfo(original).Type is { IsReferenceType: true } && Model.GetTypeInfo(original).Nullability.FlowState == NullableFlowState.NotNull) expression += "!";
            return ParseExpression(expression).WithTriviaFrom(original);
        }
        protected Cell[] DesignationCells(SyntaxNode owner) => AllCells.Values.Where(c => c.Declaration is SingleVariableDesignationSyntax && c.Declaration.Ancestors().FirstOrDefault(a => a is AnonymousFunctionExpressionSyntax or BaseMethodDeclarationSyntax or AccessorDeclarationSyntax or PropertyDeclarationSyntax or IndexerDeclarationSyntax || a is StatementSyntax and not BlockSyntax) == owner).ToArray();
        protected static StatementSyntax AllocateCell(Cell cell) => ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}();");
        private Cell[] PatternCells(SyntaxNode node) => AllCells.Values.Where(c => c.Declaration is SingleVariableDesignationSyntax && node.Span.Contains(c.Declaration.Span)).ToArray();
        private ExpressionSyntax AssignPatternCells(ExpressionSyntax condition, IEnumerable<Cell> cells)
        {
            foreach (var cell in cells)
                condition = ParseExpression("(" + condition.ToFullString() + ") && ((" + ReadCell(cell) + "=" + cell.RawName + ") is var _)");
            return condition;
        }

        public override SyntaxNode? VisitSingleVariableDesignation(SingleVariableDesignationSyntax node)
        {
            var symbol = Model.GetDeclaredSymbol(node);
            return symbol is not null && AllCells.TryGetValue(symbol, out var cell) ? node.WithIdentifier(Identifier(cell.RawName).WithTriviaFrom(node.Identifier)) : node;
        }

        public override SyntaxNode? VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            ExpressionSyntax Variable(VariableDesignationSyntax designation)
            {
                if (designation is DiscardDesignationSyntax) return IdentifierName("_");
                if (designation is ParenthesizedVariableDesignationSyntax tuple) return TupleExpression(SeparatedList(tuple.Variables.Select(v => Argument(Variable(v)))));
                var single = (SingleVariableDesignationSyntax)designation;
                var symbol = Model.GetDeclaredSymbol(single)!;
                return AllCells.TryGetValue(symbol, out var cell) ? ParseExpression(ReadCell(cell)) : DeclarationExpression(ParseTypeName(TypeName(((ILocalSymbol)symbol).Type)).WithTrailingTrivia(Space), single);
            }
            return Variable(node.Designation).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitIsPatternExpression(IsPatternExpressionSyntax node)
        {
            var rewritten = (IsPatternExpressionSyntax)base.VisitIsPatternExpression(node)!;
            return AssignPatternCells(rewritten, PatternCells(node.Pattern)).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
        {
            var rewritten = (CasePatternSwitchLabelSyntax)base.VisitCasePatternSwitchLabel(node)!;
            var cells = PatternCells(node.Pattern);
            if (cells.Length == 0) return rewritten;
            // Populate cells before user guard reads any captured pattern variables.
            var guard = AssignPatternCells(LiteralExpression(SyntaxKind.TrueLiteralExpression), cells);
            if (rewritten.WhenClause is not null) guard = ParseExpression("(" + guard.ToFullString() + ") && (" + rewritten.WhenClause.Condition.ToFullString() + ")");
            return rewritten.WithWhenClause(WhenClause(guard).WithLeadingTrivia(Space).WithTrailingTrivia(Space));
        }

        public override SyntaxNode? VisitSwitchExpressionArm(SwitchExpressionArmSyntax node)
        {
            var rewritten = (SwitchExpressionArmSyntax)base.VisitSwitchExpressionArm(node)!;
            var cells = PatternCells(node.Pattern);
            if (cells.Length == 0) return rewritten;
            var guard = AssignPatternCells(LiteralExpression(SyntaxKind.TrueLiteralExpression), cells);
            if (rewritten.WhenClause is not null) guard = ParseExpression("(" + guard.ToFullString() + ") && (" + rewritten.WhenClause.Condition.ToFullString() + ")");
            return rewritten.WithWhenClause(WhenClause(guard).WithLeadingTrivia(Space).WithTrailingTrivia(Space));
        }

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            var cells = DesignationCells(node);
            if (cells.Length == 0) return base.VisitWhileStatement(node);
            var condition = (ExpressionSyntax)Visit(node.Condition)!;
            var body = (StatementSyntax)Visit(node.Statement)!;
            var statements = cells.Select(AllocateCell).Concat([ParseStatement("if (!(" + condition.ToFullString() + ")) break;"), body]);
            return node.WithCondition(LiteralExpression(SyntaxKind.TrueLiteralExpression)).WithStatement(Block(statements));
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            var cells = DesignationCells(node);
            if (cells.Length == 0) return base.VisitDoStatement(node);
            var condition = (ExpressionSyntax)Visit(node.Condition)!;
            string label = "__n_do_condition_" + node.SpanStart;
            var continueRewriter = new ContinueRewriter(label);
            var body = (StatementSyntax)continueRewriter.Visit(Visit(node.Statement))!;
            var conditionBlock = Block(cells.Select(AllocateCell).Append(ParseStatement("if (!(" + condition.ToFullString() + ")) break;")));
            var statements = new List<StatementSyntax> { body };
            if (continueRewriter.Used) statements.Add(LabeledStatement(label, EmptyStatement()));
            statements.Add(conditionBlock);
            return WhileStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression), Block(statements)).NormalizeWhitespace().WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitCatchClause(CatchClauseSyntax node)
        {
            var rewritten = (CatchClauseSyntax)base.VisitCatchClause(node)!;
            if (node.Declaration is null || Model.GetDeclaredSymbol(node.Declaration) is not { } symbol || !AllCells.TryGetValue(symbol, out var cell)) return rewritten;
            if (node.Filter is not null)
            {
                string filter = "new " + cell.CellType + "(" + node.Declaration.Identifier.Text + ") is var " + cell.Name + " && (" + rewritten.Filter!.FilterExpression.ToFullString() + ")";
                return rewritten.WithFilter(rewritten.Filter.WithFilterExpression(ParseExpression(filter)));
            }
            return rewritten.WithBlock(rewritten.Block.WithStatements(rewritten.Block.Statements.Insert(0, ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({node.Declaration.Identifier.Text});"))));
        }

        public override SyntaxNode? VisitUsingStatement(UsingStatementSyntax node)
        {
            if (node.Declaration is null || !node.Declaration.Variables.Any(v => Model.GetDeclaredSymbol(v) is { } symbol && AllCells.ContainsKey(symbol))) return base.VisitUsingStatement(node);
            StatementSyntax body = (StatementSyntax)Visit(node.Statement)!;
            for (int i = node.Declaration.Variables.Count - 1; i >= 0; --i)
            {
                var variable = node.Declaration.Variables[i];
                var symbol = Model.GetDeclaredSymbol(variable)!;
                if (AllCells.TryGetValue(symbol, out var cell)) body = Block(ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({variable.Identifier.Text});"), body);
                var initializer = variable.Initializer is null ? null : EqualsValueClause((ExpressionSyntax)Visit(variable.Initializer.Value)!);
                body = node.WithDeclaration(node.Declaration.WithVariables(SingletonSeparatedList(variable.WithInitializer(initializer)))).WithStatement(body);
            }
            return body;
        }

        public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            var cells = PatternCells(node.Variable);
            var body = (StatementSyntax)Visit(node.Statement)!;
            if (cells.Length == 0) return node.WithExpression((ExpressionSyntax)Visit(node.Expression)!).WithStatement(body);
            var declarations = cells.Select(c => ParseStatement($"{c.CellType} {c.Name}=new {c.CellType}({c.Symbol.Name});")).ToArray();
            var block = body is BlockSyntax existing ? existing.WithStatements(existing.Statements.InsertRange(0, declarations)) : Block(declarations.Append(body));
            var loop = node.WithExpression((ExpressionSyntax)Visit(node.Expression)!).WithStatement(block);
            var collectionCells = DesignationCells(node).Where(c => node.Expression.Span.Contains(c.Declaration.Span)).ToArray();
            return collectionCells.Length == 0 ? loop : Block(collectionCells.Select(AllocateCell).Append(loop));
        }

        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            var rewritten = base.Visit(node);
            if (node is not StatementSyntax || node is BlockSyntax || rewritten is not StatementSyntax statement) return rewritten;
            var mapped = node.GetLocation().GetMappedLineSpan();
            string directive = "\n#line " + (mapped.StartLinePosition.Line + 1) + " " + Quote(mapped.Path) + "\n";
            return statement.WithLeadingTrivia(statement.GetLeadingTrivia().AddRange(ParseLeadingTrivia(directive)));
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" } && Model.GetConstantValue(node) is { HasValue: true, Value: string name })
                return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name)).WithTriviaFrom(node);
            var rewritten = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
            if (Model.GetOperation(node) is not IInvocationOperation operation) return rewritten;
            foreach (var argument in operation.Arguments)
            {
                var parameter = argument.Parameter;
                if (argument.ArgumentKind != ArgumentKind.DefaultValue || parameter is null || !parameter.GetAttributes().Any(a => a.AttributeClass?.ContainingNamespace.ToDisplayString() == "System.Runtime.CompilerServices" && a.AttributeClass.Name is "CallerArgumentExpressionAttribute" or "CallerFilePathAttribute" or "CallerLineNumberAttribute" or "CallerMemberNameAttribute")) continue;
                var constant = argument.Value.ConstantValue;
                if (!constant.HasValue) continue;
                ExpressionSyntax value = constant.Value switch
                {
                    string text => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(text)),
                    int line => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(line)),
                    _ => throw new TransformException(node, "Unsupported caller information constant.")
                };
                rewritten = rewritten.WithArgumentList(rewritten.ArgumentList.AddArguments(Argument(value).WithNameColon(NameColon(IdentifierName(parameter.Name)))));
            }
            return rewritten;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = Model.GetSymbolInfo(node).Symbol;
            if (symbol is not null && AllCells.TryGetValue(symbol, out var cell))
            {
                // nameof must remain original compile-time spelling.
                if (node.Ancestors().OfType<InvocationExpressionSyntax>().Any(i => i.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" })) return node;
                return ReadCellExpression(cell, node);
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode? VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (!node.Declaration.Variables.Any(v => Model.GetDeclaredSymbol(v) is { } symbol && AllCells.ContainsKey(symbol))) return base.VisitLocalDeclarationStatement(node);
            if (node.Declaration.Variables.Count > 1 || node.UsingKeyword.RawKind != 0) throw new TransformException(node, "Captured multi-variable and using declarations must occur directly inside a block.");
            var variable = node.Declaration.Variables[0];
            if (Model.GetDeclaredSymbol(variable) is not { } declared || !AllCells.TryGetValue(declared, out var cell)) return base.VisitLocalDeclarationStatement(node);
            if (cell.AllocateAtScopeEntry) return (variable.Initializer is null ? EmptyStatement() : ParseStatement(ReadCell(cell) + "=" + Visit(variable.Initializer.Value)!.ToFullString() + ";")).WithTriviaFrom(node);
            string value = variable.Initializer is null ? "default!" : Visit(variable.Initializer.Value)!.ToFullString();
            return ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({value});").WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitBlock(BlockSyntax node)
            => node.WithStatements(List(RewriteStatements(node.Statements)));

        public override SyntaxNode? VisitSwitchSection(SwitchSectionSyntax node)
            => node.WithLabels(List(node.Labels.Select(l => (SwitchLabelSyntax)Visit(l)!))).WithStatements(List(RewriteStatements(node.Statements, false)));

        public override SyntaxNode? VisitSwitchStatement(SwitchStatementSyntax node)
        {
            var rewritten = (SwitchStatementSyntax)base.VisitSwitchStatement(node)!;
            var cells = ScopeEntryCells(node.Sections.SelectMany(s => s.Statements));
            return cells.Length == 0 ? rewritten : Block(cells.Select(AllocateCell).Append(rewritten));
        }

        protected Cell[] ScopeEntryCells(IEnumerable<StatementSyntax> originals) => originals.OfType<LocalDeclarationStatementSyntax>()
            .Where(l => l.UsingKeyword.RawKind == 0).SelectMany(l => l.Declaration.Variables).Select(v => Model.GetDeclaredSymbol(v))
            .Where(s => s is not null && AllCells.ContainsKey(s)).Select(s => AllCells[s!]).ToArray();

        protected List<StatementSyntax> RewriteStatements(IEnumerable<StatementSyntax> originals, bool allocateScopeCells = true)
        {
            var originalStatements = originals.ToArray();
            var statements = allocateScopeCells ? ScopeEntryCells(originalStatements).Select(AllocateCell).ToList() : [];
            foreach (var statement in originalStatements)
            {
                if (statement is not (WhileStatementSyntax or DoStatementSyntax or ForStatementSyntax or ForEachVariableStatementSyntax)) statements.AddRange(DesignationCells(statement).Select(AllocateCell));
                if (statement is not LocalDeclarationStatementSyntax local || !local.Declaration.Variables.Any(v => Model.GetDeclaredSymbol(v) is { } symbol && AllCells.ContainsKey(symbol)) || local.Declaration.Variables.Count == 1 && local.UsingKeyword.RawKind == 0)
                {
                    statements.Add((StatementSyntax)Visit(statement)!);
                    continue;
                }
                foreach (var variable in local.Declaration.Variables)
                {
                    var symbol = Model.GetDeclaredSymbol(variable);
                    var initializer = variable.Initializer is null ? null : EqualsValueClause((ExpressionSyntax)Visit(variable.Initializer.Value)!);
                    if (symbol is not null && AllCells.TryGetValue(symbol, out var cell))
                    {
                        if (local.UsingKeyword.RawKind != 0)
                        {
                            // Keep C# resource disposal on the original using variable;
                            // capture aliases point at the same resource object.
                            statements.Add(local.WithDeclaration(local.Declaration.WithVariables(SingletonSeparatedList(variable.WithInitializer(initializer)))));
                            statements.Add(ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({variable.Identifier.Text});"));
                        }
                        else
                        {
                            statements.Add(initializer is null ? EmptyStatement() : ParseStatement(ReadCell(cell) + "=" + initializer.Value.ToFullString() + ";"));
                        }
                    }
                    else statements.Add(local.WithDeclaration(local.Declaration.WithVariables(SingletonSeparatedList(variable.WithInitializer(initializer)))));
                }
            }
            return statements;
        }

        public override SyntaxNode? VisitForStatement(ForStatementSyntax node)
        {
            StatementSyntax Finish(StatementSyntax statement)
            {
                var loop = statement is BlockSyntax block ? (ForStatementSyntax)block.Statements[^1] : (ForStatementSyntax)statement;
                var conditionCells = DesignationCells(node).Where(c => node.Condition?.Span.Contains(c.Declaration.Span) == true).ToArray();
                var initializerCells = DesignationCells(node).Except(conditionCells).ToArray();
                if (conditionCells.Length != 0)
                {
                    string label = "__n_increment_" + node.SpanStart;
                    var condition = loop.Condition!;
                    var continueRewriter = new ContinueRewriter(label);
                    var body = (StatementSyntax)continueRewriter.Visit(loop.Statement)!;
                    var statements = conditionCells.Select(AllocateCell).Concat([ParseStatement("if (!(" + condition.ToFullString() + ")) break;"), body])
                        .Concat(continueRewriter.Used ? [LabeledStatement(label, EmptyStatement())] : Array.Empty<StatementSyntax>())
                        .Concat(loop.Incrementors.Select(x => (StatementSyntax)ExpressionStatement(x)));
                    loop = loop.WithCondition(null).WithIncrementors(default).WithStatement(Block(statements));
                }
                if (statement is BlockSyntax existing) return existing.WithStatements(existing.Statements.Replace(existing.Statements[^1], loop).InsertRange(0, initializerCells.Select(AllocateCell)));
                return initializerCells.Length == 0 ? loop : Block(initializerCells.Select(AllocateCell).Append(loop));
            }
            if (node.Declaration is null) return Finish((StatementSyntax)base.VisitForStatement(node)!);
            var captured = node.Declaration.Variables.Where(v => Model.GetDeclaredSymbol(v) is { } s && AllCells.ContainsKey(s)).ToArray();
            if (captured.Length == 0) return Finish((StatementSyntax)base.VisitForStatement(node)!);
            if (node.Declaration.Variables.Count != 1)
            {
                var declarations = new List<StatementSyntax>();
                foreach (var initializerVariable in node.Declaration.Variables)
                {
                    var symbol = Model.GetDeclaredSymbol(initializerVariable)!;
                    string value = initializerVariable.Initializer is null ? "default!" : Visit(initializerVariable.Initializer.Value)!.ToFullString();
                    declarations.Add(AllCells.TryGetValue(symbol, out var capturedCell)
                        ? ParseStatement($"{capturedCell.CellType} {capturedCell.Name}=new {capturedCell.CellType}({value});")
                        : ParseStatement($"{TypeName(((ILocalSymbol)symbol).Type)} {initializerVariable.Identifier.Text}={value};"));
                }
                var loop = node.WithDeclaration(null).WithCondition((ExpressionSyntax?)Visit(node.Condition))
                    .WithIncrementors(SeparatedList(node.Incrementors.Select(x => (ExpressionSyntax)Visit(x)!))).WithStatement((StatementSyntax)Visit(node.Statement)!);
                return Finish(Block(declarations.Append(loop)));
            }
            var variable = captured[0];
            var cell = AllCells[Model.GetDeclaredSymbol(variable)!];
            var initializer = variable.Initializer is null ? "default!" : Visit(variable.Initializer.Value)!.ToFullString();
            var declaration = ((LocalDeclarationStatementSyntax)ParseStatement(cell.CellType + " " + cell.Name + "=new " + cell.CellType + "(" + initializer + ");")).Declaration;
            return Finish(node.WithDeclaration(declaration).WithCondition((ExpressionSyntax?)Visit(node.Condition))
                .WithIncrementors(SeparatedList(node.Incrementors.Select(x => (ExpressionSyntax)Visit(x)!)))
                .WithStatement((StatementSyntax)Visit(node.Statement)!));
        }

        private sealed class ContinueRewriter(string label) : CSharpSyntaxRewriter
        {
            public bool Used { get; private set; }
            public override SyntaxNode? VisitContinueStatement(ContinueStatementSyntax node)
            {
                Used = true;
                return ParseStatement("goto " + label + ";").WithTriviaFrom(node);
            }
            public override SyntaxNode? VisitForStatement(ForStatementSyntax node) => node;
            public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node) => node;
            public override SyntaxNode? VisitDoStatement(DoStatementSyntax node) => node;
            public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node) => node;
            public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node) => node;
            public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) => node;
            public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => node;
            public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => node;
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            var symbol = Model.GetDeclaredSymbol(node);
            if (symbol is null || !AllCells.TryGetValue(symbol, out var cell)) return base.VisitForEachStatement(node);
            var body = (StatementSyntax)Visit(node.Statement)!;
            var declaration = ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({node.Identifier.Text});");
            var block = body is BlockSyntax existing ? existing.WithStatements(existing.Statements.Insert(0, declaration)) : Block(declaration, body);
            return node.WithExpression((ExpressionSyntax)Visit(node.Expression)!).WithStatement(block);
        }

        protected BlockSyntax WithParameterCells(SyntaxNode original, BlockSyntax rewritten)
        {
            var parameters = original switch
            {
                BaseMethodDeclarationSyntax method => method.ParameterList.Parameters,
                LocalFunctionStatementSyntax local => local.ParameterList.Parameters,
                ParenthesizedLambdaExpressionSyntax lambda => lambda.ParameterList.Parameters,
                SimpleLambdaExpressionSyntax lambda => SingletonSeparatedList(lambda.Parameter),
                AnonymousMethodExpressionSyntax anonymous => anonymous.ParameterList?.Parameters ?? default,
                IndexerDeclarationSyntax indexer => indexer.ParameterList.Parameters,
                _ => default
            };
            var parameterSymbols = original is AccessorDeclarationSyntax accessor && Model.GetDeclaredSymbol(accessor) is IMethodSymbol accessorMethod
                ? accessorMethod.Parameters.Cast<ISymbol?>() : parameters.Select(p => Model.GetDeclaredSymbol(p));
            var declarations = parameterSymbols.Where(s => s is not null && AllCells.ContainsKey(s)).Select(s =>
            {
                var cell = AllCells[s!];
                return ParseStatement($"{cell.CellType} {cell.Name}=new {cell.CellType}({ParameterInitial(s!)});");
            }).Concat(DesignationCells(original).Select(AllocateCell)).ToArray();
            return rewritten.WithStatements(rewritten.Statements.InsertRange(0, declarations));
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var result = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            if (result.Body is not null) return result.WithBody(WithParameterCells(node, result.Body));
            if (result.ExpressionBody is not null && (node.ParameterList.Parameters.Any(p => AllCells.ContainsKey(Model.GetDeclaredSymbol(p)!)) || DesignationCells(node).Length != 0))
            {
                var symbol = Model.GetDeclaredSymbol(node)!;
                var statement = symbol.ReturnsVoid || symbol.IsAsync && symbol.ReturnType is INamedTypeSymbol { Name: "Task", Arity: 0 } ? (StatementSyntax)ExpressionStatement(result.ExpressionBody.Expression) : ReturnValue(result.ExpressionBody.Expression);
                return result.WithExpressionBody(null).WithSemicolonToken(default).WithBody(WithParameterCells(node, Block(statement)));
            }
            return result;
        }

        public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var result = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node)!;
            if (result.Body is not null) return result.WithBody(WithParameterCells(node, result.Body));
            if (result.ExpressionBody is not null && (node.ParameterList.Parameters.Any(p => AllCells.ContainsKey(Model.GetDeclaredSymbol(p)!)) || DesignationCells(node).Length != 0)) return result.WithExpressionBody(null).WithSemicolonToken(default).WithBody(WithParameterCells(node, Block(ExpressionStatement(result.ExpressionBody.Expression))));
            return result;
        }

        public override SyntaxNode? VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            var result = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node)!;
            if (result.Body is not null) return result.WithBody(WithParameterCells(node, result.Body));
            if (result.ExpressionBody is not null && Model.GetDeclaredSymbol(node) is IMethodSymbol method && (method.Parameters.Any(AllCells.ContainsKey) || DesignationCells(node).Length != 0))
            {
                var statement = method.ReturnsVoid ? (StatementSyntax)ExpressionStatement(result.ExpressionBody.Expression) : ReturnValue(result.ExpressionBody.Expression);
                return result.WithExpressionBody(null).WithSemicolonToken(default).WithBody(WithParameterCells(node, Block(statement)));
            }
            return result;
        }

        public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var result = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node)!;
            if (result.ExpressionBody is null || DesignationCells(node).Length == 0) return result;
            var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithBody(WithParameterCells(node, Block(ReturnValue(result.ExpressionBody.Expression))));
            return result.WithExpressionBody(null).WithSemicolonToken(default).WithAccessorList(AccessorList(SingletonList(getter)));
        }

        public override SyntaxNode? VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            var result = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node)!;
            if (result.ExpressionBody is null || DesignationCells(node).Length == 0 && !node.ParameterList.Parameters.Any(p => AllCells.ContainsKey(Model.GetDeclaredSymbol(p)!))) return result;
            var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithBody(WithParameterCells(node, Block(ReturnValue(result.ExpressionBody.Expression))));
            return result.WithExpressionBody(null).WithSemicolonToken(default).WithAccessorList(AccessorList(SingletonList(getter)));
        }

        public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var result = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!;
            if (result.Body is not null) return result.WithBody(WithParameterCells(node, result.Body));
            if (result.ExpressionBody is not null && (node.ParameterList.Parameters.Any(p => AllCells.ContainsKey(Model.GetDeclaredSymbol(p)!)) || DesignationCells(node).Length != 0))
            {
                var symbol = Model.GetDeclaredSymbol(node)!;
                var statement = symbol.ReturnsVoid ? (StatementSyntax)ExpressionStatement(result.ExpressionBody.Expression) : ReturnValue(result.ExpressionBody.Expression);
                return result.WithExpressionBody(null).WithSemicolonToken(default).WithBody(WithParameterCells(node, Block(statement)));
            }
            return result;
        }

        public override SyntaxNode? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var result = (SimpleLambdaExpressionSyntax)base.VisitSimpleLambdaExpression(node)!;
            return result.WithBody(LambdaBody(node, result.Body));
        }

        public override SyntaxNode? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            var result = (ParenthesizedLambdaExpressionSyntax)base.VisitParenthesizedLambdaExpression(node)!;
            return result.WithBody(LambdaBody(node, result.Body));
        }

        private CSharpSyntaxNode LambdaBody(LambdaExpressionSyntax original, CSharpSyntaxNode body)
        {
            if (body is BlockSyntax block) return WithParameterCells(original, block);
            var parameterSymbols = original switch
            {
                SimpleLambdaExpressionSyntax simple => new[] { Model.GetDeclaredSymbol(simple.Parameter) },
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters.Select(p => Model.GetDeclaredSymbol(p)).ToArray(),
                _ => []
            };
            if (!parameterSymbols.Any(s => s is not null && AllCells.ContainsKey(s)) && DesignationCells(original).Length == 0) return body;
            var method = (IMethodSymbol?)Model.GetSymbolInfo(original).Symbol;
            bool noReturn = method?.ReturnsVoid == true || method is { IsAsync: true, ReturnType: INamedTypeSymbol { Name: "Task", Arity: 0 } };
            var statement = noReturn ? (StatementSyntax)ExpressionStatement((ExpressionSyntax)body) : ReturnValue((ExpressionSyntax)body);
            return WithParameterCells(original, Block(statement));
        }

        public override SyntaxNode? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            var result = (AnonymousMethodExpressionSyntax)base.VisitAnonymousMethodExpression(node)!;
            return result.WithBlock(WithParameterCells(node, result.Block));
        }
    }

    private sealed class SourceRewriter : CellRewriter
    {
        private readonly Dictionary<InvocationExpressionSyntax, Callback> _callbacks;
        private readonly Dictionary<TypeDeclarationSyntax, string> _ownerInitializers = [];
        private readonly List<string> _entryPoints = [];
        private readonly string _bridge;
        private readonly string _top;

        public SourceRewriter(SemanticModel model, List<Callback> callbacks, Dictionary<ISymbol, Cell> cells, string bridge) : base(model, cells)
        { _callbacks = callbacks.ToDictionary(c => c.Invocation); _bridge = bridge; _top = bridge + "_Top"; }

        public override SyntaxNode? VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var members = new List<MemberDeclarationSyntax>();
            members.AddRange(AllCells.Values.Where(c => c.Declaration is CompilationUnitSyntax).Select(c => GlobalStatement(ParseStatement($"{c.CellType} {c.Name}=new {c.CellType}({c.Symbol.Name});"))));
            members.AddRange(ScopeEntryCells(node.Members.OfType<GlobalStatementSyntax>().Select(g => g.Statement)).Select(c => GlobalStatement(AllocateCell(c))));
            foreach (var member in node.Members)
            {
                if (member is GlobalStatementSyntax global) members.AddRange(RewriteStatements([global.Statement], false).Select(GlobalStatement));
                else members.Add((MemberDeclarationSyntax)Visit(member)!);
            }
            return node.WithMembers(List(members));
        }

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (!_callbacks.TryGetValue(node, out var callback)) return base.VisitInvocationExpression(node);
            return ParseExpression(callback.CreateExpression(Visit, qualification: callback.Owner is null ? _top + "." : null)).WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var rewritten = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!;
            if (Model.GetDeclaredSymbol(node) is not { } symbol) return rewritten;
            bool originalNameOfUse = Model.SyntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().Any(identifier => identifier.Ancestors().OfType<InvocationExpressionSyntax>().Any(i => i.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" }) && SymbolEqualityComparer.Default.Equals(Model.GetSymbolInfo(identifier).Symbol, symbol));
            if (!originalNameOfUse && !_callbacks.Values.Any(c => c.LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, symbol.OriginalDefinition)))) return rewritten;
            return rewritten.WithLeadingTrivia(rewritten.GetLeadingTrivia().AddRange(ParseLeadingTrivia("\n#pragma warning disable CS8321\n")))
                .WithTrailingTrivia(rewritten.GetTrailingTrivia().AddRange(ParseTrailingTrivia("\n#pragma warning restore CS8321\n")));
        }

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) => RewriteType(node, (TypeDeclarationSyntax)base.VisitClassDeclaration(node)!);
        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) => RewriteType(node, (TypeDeclarationSyntax)base.VisitStructDeclaration(node)!);
        public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) => RewriteType(node, (TypeDeclarationSyntax)base.VisitRecordDeclaration(node)!);
        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => RewriteType(node, (TypeDeclarationSyntax)base.VisitInterfaceDeclaration(node)!);

        private TypeDeclarationSyntax RewriteType(TypeDeclarationSyntax original, TypeDeclarationSyntax rewritten)
        {
            var own = _callbacks.Values.Where(c => c.Owner == original).ToArray();
            var children = _ownerInitializers.Where(pair => pair.Key.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault() == original).ToArray();
            if (own.Length == 0 && children.Length == 0) return rewritten;
            var initializer = "__NIndirectInitialize_" + Hash(original.SyntaxTree.FilePath) + "_" + original.SpanStart;
            _ownerInitializers.Add(original, initializer);
            var members = own.SelectMany(c => c.GenerateMembers(_callbacks)).ToList();
            var calls = own.Select(c => c.InitName + ".Register();").Concat(children.Select(c => c.Key.Identifier.Text + "." + c.Value + ".Register();"));
            // Nested registration classes can access private thunks without running a
            // user owner's static constructor during module bootstrap.
            members.Add(ParseMemberDeclaration($"public static class {initializer} {{ public static void Register(){{{string.Concat(calls)}}} }}")!);
            if (!original.Ancestors().OfType<TypeDeclarationSyntax>().Any()) _entryPoints.Add(TypeName((INamedTypeSymbol)Model.GetDeclaredSymbol(original)!) + "." + initializer + ".Register();");
            return rewritten.AddMembers(members.Select(m => m.NormalizeWhitespace().WithLeadingTrivia(ParseLeadingTrivia("\n#nullable enable\n#line hidden\n")).WithTrailingTrivia(ParseTrailingTrivia("\n"))).ToArray())
                .WithTrailingTrivia(rewritten.GetTrailingTrivia().AddRange(ParseTrailingTrivia(NullableRestore(original) + "\n#line " + (original.GetLocation().GetLineSpan().EndLinePosition.Line + 1) + " " + Quote(original.SyntaxTree.FilePath) + "\n")));
        }

        private string NullableRestore(SyntaxNode node)
        {
            var context = Model.GetNullableContext(node.Span.End);
            return "\n#nullable " + (context.HasFlag(NullableContext.AnnotationsEnabled) ? "enable" : "disable") + " annotations\n#nullable " + (context.HasFlag(NullableContext.WarningsEnabled) ? "enable" : "disable") + " warnings";
        }

        public IEnumerable<MemberDeclarationSyntax> BuildTopLevelMembers()
        {
            var top = _callbacks.Values.Where(c => c.Owner is null).ToArray();
            if (top.Length != 0)
            {
                var members = top.SelectMany(c => c.GenerateMembers(_callbacks)).Select(m => m is ClassDeclarationSyntax cls ? cls.WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword))) : m).ToArray();
                var calls = string.Concat(top.Select(c => c.InitName + ".Register();"));
                var init = ParseMemberDeclaration($"public static void Initialize(){{{calls}}}")!;
                yield return ClassDeclaration(_top).WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))).AddMembers(members.Append(init).ToArray()).NormalizeWhitespace().WithLeadingTrivia(ParseLeadingTrivia("\n#line hidden\n"));
                _entryPoints.Add(_top + ".Initialize();");
            }
            string bridgeCode = $"public static class {_bridge} {{ public static void Register(){{{string.Concat(_entryPoints)}}} public static void Initialize()=>Register(); }}";
            yield return ParseMemberDeclaration(bridgeCode)!.NormalizeWhitespace().WithLeadingTrivia(ParseLeadingTrivia("\n#line hidden\n"));
        }
    }

    private sealed class ThunkRewriter : CellRewriter
    {
        private readonly Callback _callback;
        private readonly Dictionary<InvocationExpressionSyntax, Callback> _callbacks;
        private readonly bool _instanceBody;
        public Dictionary<ISymbol, string> Parameters { get; } = new(SymbolEqualityComparer.Default);
        public ThunkRewriter(Callback callback, Dictionary<InvocationExpressionSyntax, Callback> callbacks, bool instanceBody = false) : base(callback.Model, callbacks.Values.SelectMany(c => c.Cells).GroupBy(p => p.Key, SymbolEqualityComparer.Default).ToDictionary(g => g.Key, g => g.First().Value, SymbolEqualityComparer.Default))
        { _callback = callback; _callbacks = callbacks; _instanceBody = instanceBody; }

        public BlockSyntax RewriteBodyParameters(SyntaxNode original, BlockSyntax rewritten) => WithParameterCells(original, rewritten);
        protected override string ParameterInitial(ISymbol symbol) => Parameters.GetValueOrDefault(symbol, symbol.Name);

        protected override string ReadCell(Cell cell) => _callback.Cells.ContainsKey(cell.Symbol) ? "__env." + _callback.CellField(cell) + ".Value" : cell.Access;
        protected override string CellReference(Cell cell) => _callback.Cells.ContainsKey(cell.Symbol) ? "__env." + _callback.CellField(cell) : cell.Name;

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = Model.GetSymbolInfo(node).Symbol;
            if (symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } local && _callback.LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, local.OriginalDefinition))) return AdaptLocalMethodGroup(node, local);
            if (symbol is ILocalSymbol { IsConst: true, HasConstantValue: true } constant && constant.DeclaringSyntaxReferences.First().GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                return CastExpression(ParseTypeName(TypeName(constant.Type)), ParenthesizedExpression((ExpressionSyntax)Visit(initializer.Value)!)).WithTriviaFrom(node);
            if (symbol is not null && Parameters.TryGetValue(symbol, out var parameter))
            {
                if (AllCells.TryGetValue(symbol, out var cell)) return ReadCellExpression(cell, node);
                return ParseExpression(parameter).WithTriviaFrom(node);
            }
            if (!_instanceBody && _callback.NeedsThis && symbol is IFieldSymbol { IsStatic: false } or IPropertySymbol { IsStatic: false } or IEventSymbol { IsStatic: false } or IMethodSymbol { IsStatic: false, MethodKind: not MethodKind.LocalFunction })
            {
                if (node.Parent is not MemberAccessExpressionSyntax access || access.Name != node)
                    return ParseExpression("__env.__this." + node.ToFullString()).WithTriviaFrom(node);
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
        {
            if (Model.GetSymbolInfo(node).Symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } local && _callback.LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, local.OriginalDefinition))) return AdaptLocalMethodGroup(node, local);
            if (!_instanceBody && _callback.NeedsThis && Model.GetSymbolInfo(node).Symbol is IMethodSymbol { IsStatic: false } && (node.Parent is not MemberAccessExpressionSyntax access || access.Name != node))
                return ParseExpression("__env.__this." + node.ToFullString()).WithTriviaFrom(node);
            return base.VisitGenericName(node);
        }

        private SyntaxNode AdaptLocalMethodGroup(SimpleNameSyntax node, IMethodSymbol local)
        {
            if (Model.GetTypeInfo(node).ConvertedType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod: { } invoke } target) throw new TransformException(node, "Local function reference requires a concrete delegate conversion.");
            var declarations = invoke.Parameters.Select((p, i) => (p.RefKind == RefKind.None ? "" : p.RefKind.ToString().ToLowerInvariant() + " ") + TypeName(p.Type) + " __nlocal_arg" + i);
            var arguments = invoke.Parameters.Select((p, i) => (p.RefKind == RefKind.None ? "" : p.RefKind.ToString().ToLowerInvariant() + " ") + "__nlocal_arg" + i);
            var syntax = local.DeclaringSyntaxReferences.First().GetSyntax();
            string typeArguments = local.IsGenericMethod ? "<" + string.Join(",", local.TypeArguments.Select(TypeName)) + ">" : "";
            string call = _callback.Name + "_Local_" + syntax.SpanStart + typeArguments + "(" + (_callback.HasEnvironment ? "__env" : "null") + string.Concat(arguments.Select(a => "," + a)) + ")";
            // Cast to the original semantic target so lambda numeric conversions do
            // not change overload selection from the original method group.
            return ParseExpression("((" + TypeName(target) + ")((" + string.Join(",", declarations) + ") => " + call + "))").WithTriviaFrom(node);
        }

        public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var rewritten = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!;
            if (Model.GetDeclaredSymbol(node) is not { } symbol || !_callbacks.Values.Any(c => c.LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, symbol.OriginalDefinition)))) return rewritten;
            return rewritten.WithLeadingTrivia(rewritten.GetLeadingTrivia().AddRange(ParseLeadingTrivia("\n#pragma warning disable CS8321\n")))
                .WithTrailingTrivia(rewritten.GetTrailingTrivia().AddRange(ParseTrailingTrivia("\n#pragma warning restore CS8321\n")));
        }

        public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node) => !_instanceBody && _callback.NeedsThis ? ParseExpression("__env.__this").WithTriviaFrom(node) : node;
        public override SyntaxNode? VisitBaseExpression(BaseExpressionSyntax node) => _instanceBody ? node : throw new TransformException(node, "Base receiver cannot be lowered outside its declaring type.");

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (_callbacks.TryGetValue(node, out var nested))
            {
                var creation = nested.CreateExpression(Visit, CellReference);
                if (nested.NeedsThis && _callback.NeedsThis) creation = creation.Replace("__this=this", "__this=__env.__this", StringComparison.Ordinal);
                return ParseExpression(creation).WithTriviaFrom(node);
            }
            if (Model.GetSymbolInfo(node).Symbol is IMethodSymbol { MethodKind: MethodKind.LocalFunction } local && _callback.LocalFunctions.Any(f => SymbolEqualityComparer.Default.Equals(f, local.OriginalDefinition)))
            {
                var syntax = local.DeclaringSyntaxReferences.First().GetSyntax();
                var args = node.ArgumentList.Arguments.Select(a => a.WithExpression((ExpressionSyntax)Visit(a.Expression)!)).ToList();
                args.Insert(0, Argument(ParseExpression(_callback.HasEnvironment ? "__env" : "null")));
                string helper = _callback.Name + "_Local_" + syntax.SpanStart;
                SimpleNameSyntax name = node.Expression is GenericNameSyntax generic ? GenericName(Identifier(helper), generic.TypeArgumentList) : IdentifierName(helper);
                return InvocationExpression(name, ArgumentList(SeparatedList(args))).WithTriviaFrom(node);
            }
            return base.VisitInvocationExpression(node);
        }
    }
}
