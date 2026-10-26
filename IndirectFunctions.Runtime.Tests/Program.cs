using IndirectFunctions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

static class RuntimeTests
{
    static int assertions;
    static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}; actual {actual}"); assertions++; }
    static void Throws<T>(Action action) where T:Exception { try { action(); } catch(T) { assertions++; return; } throw new Exception("Expected " + typeof(T)); }
    static void Unmanaged<T>() where T:unmanaged
    {
        Equal(false,RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        Equal(32,Unsafe.SizeOf<T>());
    }
    static void Managed<T>() => Equal(true,RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    static long LongIdentity(object? state,long value)=>value;
    static int wideSum;
    static int Sum16(object? state,int a,int b,int c,int d,int e,int f,int g,int h,int i,int j,int k,int l,int m,int n,int o,int p)=>a+b+c+d+e+f+g+h+i+j+k+l+m+n+o+p;
    static void Set16(object? state,int a,int b,int c,int d,int e,int f,int g,int h,int i,int j,int k,int l,int m,int n,int o,int p)=>wideSum=Sum16(state,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p);
    static NCaptureGraph ReplaceRootFields(NCaptureGraph graph,params NCaptureField[] fields)=>new(graph.Nodes.Select(node=>node.NodeId==graph.Roots[0]?node with {Fields=fields}:node).ToArray(),graph.Roots);

    static int Add(object? state, int x) => ((Root)state!).Cell.Value += x;
    static int Identity(object? state, int x) => x;
    static void Set(object? state, int x) => ((Root)state!).Cell.Value = x;
    static unsafe void Main()
    {
        var id = NCodeId.FromKey("probe.add");
        Equal(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("IndirectFunctions.CodeId.v1\0probe.add"))).Substring(0,32).ToLowerInvariant(), id.ToString());
        Equal(id,NCodeId.FromBytes(id.ToBytes()));
        Throws<ArgumentException>(()=>NCodeId.FromBytes(new byte[15]));
        Unmanaged<NFunc<string>>();
        Managed<ManagedNFunc<string>>();
        Unmanaged<NFunc<object,string>>();
        Managed<ManagedNFunc<object,string>>();
        Unmanaged<NFunc<object,object,string>>();
        Managed<ManagedNFunc<object,object,string>>();
        Unmanaged<NFunc<object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Managed<ManagedNFunc<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,string>>();
        Unmanaged<NAction>();
        Managed<ManagedNAction>();
        Unmanaged<NAction<object>>();
        Managed<ManagedNAction<object>>();
        Unmanaged<NAction<object,object>>();
        Managed<ManagedNAction<object,object>>();
        Unmanaged<NAction<object,object,object>>();
        Managed<ManagedNAction<object,object,object>>();
        Unmanaged<NAction<object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Unmanaged<NAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Managed<ManagedNAction<object,object,object,object,object,object,object,object,object,object,object,object,object,object,object,object>>();
        Equal(false,RuntimeHelpers.IsReferenceOrContainsReferences<NFunc<object,int>>());
        var schema = new NCaptureSchema("probe.root.v1",static ()=>new Root());
        NGenerated.RegisterCaptureSchema(schema);
        NFunc<int,int>.RegisterGenerated("probe.add", &Add,schema.Id);
        NFunc<int,int>.RegisterGenerated("probe.identity", &Identity);
        NAction<int>.RegisterGenerated("probe.set", &Set,schema.Id);
        var context=new NContext(); var other=new NContext();
        var cell=new NCell<int>(10);
        var root=new Root{Cell=cell}; root.Next=root;
        var managed=ManagedNFunc<int,int>.CreateRegistered(id,root);
        var callback=context.Import(managed); var callbackOther=other.Import(managed);
        Equal(12,callback.Invoke(context,2)); Equal(15,managed.Invoke(3)); Equal(19,callbackOther.Invoke(other,4));
        var copied=callback; Equal(20,copied.Invoke(context,1));
        Throws<InvalidOperationException>(()=>callback.Invoke(other,1));
        var unbound=NFunc<int,int>.FromBytes(callback.ToBytes());
        Equal(false,unbound.IsBound); Throws<InvalidOperationException>(()=>unbound.Invoke(context,1));
        Equal(21,unbound.Bind(context,callback.CaptureRef).Invoke(context,1));
        var ref2=context.ImportCapture(new Root{Cell=cell});
        var graph=context.ExportCaptures(callback.CaptureRef,ref2);
        Equal(3,graph.Nodes.Count); Equal(2,graph.Roots.Count);
        var restoredContext=new NContext();
        var refs=restoredContext.ImportCaptures(graph);
        var restored1=(Root)restoredContext.ResolveCapture(refs[0]); var restored2=(Root)restoredContext.ResolveCapture(refs[1]);
        Equal(true,ReferenceEquals(restored1.Cell,restored2.Cell)); Equal(true,ReferenceEquals(restored1.Next,restored1));
        Equal(22,unbound.Bind(restoredContext,refs[0]).Invoke(restoredContext,1)); Equal(22,restored2.Cell.Value);
        Equal(21,cell.Value);
        context.Release(callback.CaptureRef); Throws<InvalidOperationException>(()=>copied.Invoke(context,1));
        Equal(22,((Root)context.ResolveCapture(ref2)).Cell.Value+=1);
        var fresh=context.ImportCapture(root); Throws<InvalidOperationException>(()=>context.ResolveCapture(callback.CaptureRef)); Equal(true,fresh!=callback.CaptureRef);
        context.Dispose(); Throws<ObjectDisposedException>(()=>context.ResolveCapture(fresh)); Equal(23,managed.Invoke(1)); Equal(24,callbackOther.Invoke(other,1));
        var stateless=NFunc<int,int>.FromCodeId(NCodeId.FromKey("probe.identity")); Equal(42,stateless.Invoke(other,42));
        var warm=callbackOther;
        for(int i=0;i<10000;i++) warm.Invoke(other,0);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<100000;i++) warm.Invoke(other,0);
        Equal(0L,GC.GetAllocatedBytesForCurrentThread()-before);
        Throws<InvalidOperationException>(()=>NFunc<long,long>.FromBytes(id.ToBytes()));
        Throws<InvalidOperationException>(()=>ManagedNFunc<int,int>.Create("marker",x=>x));
        Throws<InvalidOperationException>(()=>stateless.Bind(other,refs[0]));

        Throws<InvalidOperationException>(()=>default(NFunc<int,int>).Invoke(other,1));
        Throws<InvalidOperationException>(()=>NFunc<int,int>.FromBytes(new byte[16]));
        Throws<InvalidOperationException>(()=>NFunc<long,long>.RegisterGenerated("probe.identity",&LongIdentity));
        NFunc<int,int>.RegisterGenerated("probe.identity",&Identity);
        Throws<InvalidOperationException>(()=>NFunc<int,int>.RegisterGenerated("probe.identity",&Add,schema.Id));
        var managedUnbound=ManagedNFunc<int,int>.FromBytes(id.ToBytes());
        Equal(false,managedUnbound.IsBound);
        Throws<InvalidOperationException>(()=>managedUnbound.Invoke(1));
        Equal(25,managedUnbound.Bind(root).Invoke(1));
        var badRef=other.ImportCapture(new NCell<int>());
        Throws<InvalidOperationException>(()=>callbackOther.Bind(other,badRef));
        Throws<InvalidOperationException>(()=>NFunc<int,int>.CreateRegistered(other,id,new NCell<int>()));
        Throws<InvalidOperationException>(()=>ManagedNFunc<int,int>.CreateRegistered(id,new NCell<int>()));
        Throws<InvalidOperationException>(()=>NGenerated.RegisterCaptureSchema(new NCaptureSchema(schema.Key,static()=>new Root())));
        Throws<ArgumentNullException>(()=>restoredContext.ImportCaptures(null!));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(new NCaptureGraph([graph.Nodes[0],graph.Nodes[0]],[graph.Roots[0]])));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(new NCaptureGraph(graph.Nodes,[int.MaxValue])));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,NCodeId.FromKey("missing.schema"),[])],[1])));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(ReplaceRootFields(graph,new NCaptureField("cell",NCaptureValue.FromLeaf(123)),new NCaptureField("next",NCaptureValue.FromLeaf(null)))));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(ReplaceRootFields(graph)));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(ReplaceRootFields(graph,new NCaptureField("cell",NCaptureValue.FromReference(999999)))));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(ReplaceRootFields(graph,new NCaptureField("cell",new NCaptureValue((NCaptureValueKind)123,null,0)))));
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(ReplaceRootFields(graph,new NCaptureField("cell",NCaptureValue.FromLeaf(null)),new NCaptureField("cell",NCaptureValue.FromLeaf(null)))));
        var wrongSchema=new NCaptureSchema("probe.wrongfactory",static()=>new Root());
        NGenerated.RegisterCaptureSchema(wrongSchema);
        Throws<InvalidOperationException>(()=>restoredContext.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,wrongSchema.Id,[])],[1])));
        using(var registration=NGenerated.BeginRegistrationBatch())
        {
            NFunc<int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int>.RegisterGenerated("probe.widefunc",&Sum16);
            using(var nested=NGenerated.BeginRegistrationBatch())
                NAction<int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int>.RegisterGenerated("probe.wideaction",&Set16);
            Equal(136,NFunc<int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int>.FromCodeId(NCodeId.FromKey("probe.widefunc")).Invoke(other,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16));
        }
        NAction<int,int,int,int,int,int,int,int,int,int,int,int,int,int,int,int>.FromCodeId(NCodeId.FromKey("probe.wideaction")).Invoke(other,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16);
        Equal(136,wideSum);
        var outer=NGenerated.BeginRegistrationBatch(); var inner=NGenerated.BeginRegistrationBatch();
        Throws<InvalidOperationException>(()=>outer.Dispose()); inner.Dispose(); outer.Dispose();
        var publishing=Task.Run(()=>{using var batch=NGenerated.BeginRegistrationBatch();for(int i=0;i<1000;i++)NFunc<int,int>.RegisterGenerated("probe.concurrent."+i,&Identity);});
        for(int i=0;i<10000;i++)Equal(i,stateless.Invoke(other,i));
        publishing.GetAwaiter().GetResult();
        Equal(999,NFunc<int,int>.FromCodeId(NCodeId.FromKey("probe.concurrent.999")).Invoke(other,999));
        var deepRoot=new Root {Cell=cell}; var cursor=deepRoot;
        for(int i=0;i<10000;i++){cursor.Next=new Root {Cell=cell};cursor=cursor.Next;}
        var deepRef=other.ImportCapture(deepRoot);
        var deepGraph=other.ExportCaptures(deepRef);
        Equal(10002,deepGraph.Nodes.Count);
        var deepRestored=restoredContext.ImportCaptures(deepGraph);
        var deepState=(Root)restoredContext.ResolveCapture(deepRestored[0]);
        for(int i=0;i<10000;i++){Equal(true,ReferenceEquals(deepState.Cell,deepState.Next!.Cell));deepState=deepState.Next!;}
        Equal(null,deepState.Next);
        CodecTests(other,restoredContext);
        RootAliasTests(other,restoredContext);
        Console.WriteLine($"runtime assertions={assertions}; invocation allocations=0; holder bytes={Unsafe.SizeOf<NFunc<int,int>>()}" );
    }

    static void CodecTests(NContext source,NContext destination)
    {
        NGenerated.RegisterCaptureSchema(new NCaptureSchema("probe.codec.scalars.v1",static()=>new ScalarState()));
        var cell=new NCell<int>(23);
        var state=new ScalarState {Number=17,Optional=null,Name="typed leaf",Cell=cell,CellAsObject=cell};
        var token=source.ImportCapture(state);
        var encoder=new ScalarCodec();
        var graph=source.ExportCaptures(new[]{token},encoder);
        Equal(2,graph.Nodes.Count);
        Equal(4,encoder.EncodeCalls.Count);
        Equal(0,encoder.DecodeCalls.Count);
        Equal(true,encoder.EncodeCalls.All(type=>type==typeof(int)||type==typeof(string)));
        Equal(1,encoder.NullEncodes);
        // Encoded representations implementing INCaptureState must remain leaves, not become graph nodes.
        Equal(true,graph.Nodes.SelectMany(node=>node.Fields).Where(field=>field.Value.Kind==NCaptureValueKind.Leaf).All(field=>field.Value.Leaf is ScalarToken));
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(graph));
        var decoder=new ScalarCodec();
        var restored=(ScalarState)destination.ResolveCapture(destination.ImportCaptures(graph,decoder)[0]);
        Equal(17,restored.Number);
        Equal(null,restored.Optional);
        Equal("typed leaf",restored.Name);
        Equal(23,restored.Cell.Value);
        Equal(false,ReferenceEquals(cell,restored.Cell));
        Equal(true,ReferenceEquals(restored.Cell,restored.CellAsObject));
        Equal(4,decoder.DecodeCalls.Count);
        Equal(0,decoder.EncodeCalls.Count);
        Equal(1,decoder.NullDecodes);

        NGenerated.RegisterCaptureSchema(new NCaptureSchema("probe.codec.objects.v1",static()=>new ObjectState()));
        var leaf=new OrdinaryLeaf {Value=42};
        var roots=new[]{source.ImportCapture(new ObjectState {Left=leaf,Right=leaf}),source.ImportCapture(new ObjectState {Left=leaf,Right=leaf})};
        var objectGraph=source.ExportCaptures(roots,new IdentityCodec());
        var objectRefs=destination.ImportCaptures(objectGraph,new IdentityCodec());
        var first=(ObjectState)destination.ResolveCapture(objectRefs[0]);
        var second=(ObjectState)destination.ResolveCapture(objectRefs[1]);
        Equal(true,ReferenceEquals(first.Left,first.Right));
        Equal(true,ReferenceEquals(first.Left,second.Left));
        Equal(true,ReferenceEquals(second.Left,second.Right));
        Equal(false,ReferenceEquals(leaf,first.Left));
        Equal(42,((OrdinaryLeaf)first.Left!).Value);
        Throws<FormatException>(()=>source.ExportCaptures(new[]{token},new RejectCodec()));
        Throws<FormatException>(()=>destination.ImportCaptures(graph,new RejectCodec()));
    }

    static void RootAliasTests(NContext source,NContext destination)
    {
        var state=new Root {Cell=new NCell<int>(7)};
        var first=source.ImportCapture(state);
        var independent=source.ImportCapture(state);
        var graph=source.ExportCaptures(first,first,independent);
        Equal(3,graph.Roots.Count);
        Equal(graph.Roots[0],graph.Roots[1]);
        Equal(graph.Roots[0],graph.Roots[2]);
        Equal(graph.RootIds![0],graph.RootIds[1]);
        Equal(false,graph.RootIds[0]==graph.RootIds[2]);
        var imported=destination.ImportCaptures(graph);
        Equal(imported[0],imported[1]);
        Equal(false,imported[0]==imported[2]);
        Equal(true,ReferenceEquals(destination.ResolveCapture(imported[0]),destination.ResolveCapture(imported[2])));
        destination.Release(imported[0]);
        Throws<InvalidOperationException>(()=>destination.ResolveCapture(imported[1]));
        Equal(7,((Root)destination.ResolveCapture(imported[2])).Cell.Value);
        var legacy=destination.ImportCaptures(new NCaptureGraph(graph.Nodes,graph.Roots));
        Equal(false,legacy[0]==legacy[1]);
        destination.Release(legacy[0]);
        Equal(7,((Root)destination.ResolveCapture(legacy[1])).Cell.Value);
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(graph with {RootIds=[1]}));
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(graph with {RootIds=[1,0,2]}));
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(graph with {RootIds=[1,-1,2]}));
        var different=source.ImportCapture(new Root());
        var differentGraph=source.ExportCaptures(first,different);
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(differentGraph with {RootIds=[1,1]}));
        Throws<ArgumentNullException>(()=>destination.ImportCaptures(new NCaptureGraph(null!,[])));
        Throws<ArgumentNullException>(()=>destination.ImportCaptures(new NCaptureGraph([],null!)));
        Throws<ArgumentNullException>(()=>destination.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,state.CaptureSchemaId,null!)],[1])));
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(new NCaptureGraph([null!],[])));
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,state.CaptureSchemaId,[null!])],[1])));
        var reusedSchema=new NCaptureSchema("probe.reused.factory.v1",static()=>ReusedState.Singleton);
        NGenerated.RegisterCaptureSchema(reusedSchema);
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,reusedSchema.Id,[]),new NCaptureNode(2,reusedSchema.Id,[])],[1,2])));
        var nullSchema=new NCaptureSchema("probe.null.factory.v1",static()=>null!);
        NGenerated.RegisterCaptureSchema(nullSchema);
        Throws<InvalidOperationException>(()=>destination.ImportCaptures(new NCaptureGraph([new NCaptureNode(1,nullSchema.Id,[])],[1])));
    }

    sealed class RejectCodec:INCaptureLeafCodec
    {
        public object? Encode<T>(T value)=>throw new FormatException("Rejected leaf.");
        public T Decode<T>(object? encoded)=>throw new FormatException("Rejected leaf.");
    }

    sealed class ReusedState:INCaptureState
    {
        public static readonly ReusedState Singleton=new();
        static readonly NCodeId Id=NCodeId.FromKey("probe.reused.factory.v1");
        public NCodeId CaptureSchemaId=>Id;
        public void Write(NCaptureWriter writer){}
        public void Read(NCaptureReader reader){}
    }

    sealed class ScalarState:INCaptureState
    {
        static readonly NCodeId Id=NCodeId.FromKey("probe.codec.scalars.v1");
        public int Number;public string? Optional;public string Name="";public NCell<int> Cell=new();public object? CellAsObject;
        public NCodeId CaptureSchemaId=>Id;
        public void Write(NCaptureWriter writer){writer.Field("number",Number);writer.Field("optional",Optional);writer.Field("name",Name);writer.Field("cell",Cell);writer.Field("objectCell",CellAsObject);}
        public void Read(NCaptureReader reader){Number=reader.Field<int>("number");Optional=reader.Field<string?>("optional");Name=reader.Field<string>("name");Cell=reader.Field<NCell<int>>("cell");CellAsObject=reader.Field<object?>("objectCell");}
    }

    sealed class ScalarToken(string? type,string? content):INCaptureState
    {
        public readonly string? TypeName=type;
        public readonly string? Content=content;
        public NCodeId CaptureSchemaId=>throw new Exception("Encoded token incorrectly treated as capture node.");
        public void Write(NCaptureWriter writer)=>throw new Exception("Encoded token incorrectly visited as capture node.");
        public void Read(NCaptureReader reader)=>throw new Exception("Encoded token incorrectly materialized as capture node.");
    }

    sealed class ScalarCodec:INCaptureLeafCodec
    {
        public readonly List<Type> EncodeCalls=[];public readonly List<Type> DecodeCalls=[];public int NullEncodes;public int NullDecodes;
        public object? Encode<T>(T value)
        {
            EncodeCalls.Add(typeof(T));
            if(value is null){NullEncodes++;return new ScalarToken(typeof(T).FullName,null);}
            if(value is int integer)return new ScalarToken(typeof(T).FullName,integer.ToString(CultureInfo.InvariantCulture));
            if(value is string text)return new ScalarToken(typeof(T).FullName,text);
            throw new InvalidOperationException("Unsupported scalar type.");
        }
        public T Decode<T>(object? encoded)
        {
            DecodeCalls.Add(typeof(T));
            var token=(ScalarToken)encoded!;
            Equal(typeof(T).FullName,token.TypeName);
            if(token.Content is null){NullDecodes++;return default!;}
            if(typeof(T)==typeof(int))return (T)(object)int.Parse(token.Content,CultureInfo.InvariantCulture);
            if(typeof(T)==typeof(string))return (T)(object)token.Content;
            throw new InvalidOperationException("Unsupported scalar type.");
        }
    }

    sealed class OrdinaryLeaf {public int Value;}
    sealed record LeafReference(int Id,int Value);
    sealed class ObjectState:INCaptureState
    {
        static readonly NCodeId Id=NCodeId.FromKey("probe.codec.objects.v1");
        public object? Left;public object? Right;
        public NCodeId CaptureSchemaId=>Id;
        public void Write(NCaptureWriter writer){writer.Field("left",Left);writer.Field("right",Right);}
        public void Read(NCaptureReader reader){Left=reader.Field<object?>("left");Right=reader.Field<object?>("right");}
    }
    sealed class IdentityCodec:INCaptureLeafCodec
    {
        readonly Dictionary<object,int> encoded=new(ReferenceEqualityComparer.Instance);
        readonly Dictionary<int,OrdinaryLeaf> decoded=[];
        public object? Encode<T>(T value)
        {
            if(value is null)return null;
            var leaf=(OrdinaryLeaf)(object)value;
            if(!encoded.TryGetValue(leaf,out var id)){id=encoded.Count+1;encoded.Add(leaf,id);}
            return new LeafReference(id,leaf.Value);
        }
        public T Decode<T>(object? value)
        {
            if(value is null)return default!;
            var reference=(LeafReference)value;
            if(!decoded.TryGetValue(reference.Id,out var leaf)){leaf=new OrdinaryLeaf {Value=reference.Value};decoded.Add(reference.Id,leaf);}
            return (T)(object)leaf;
        }
    }
    sealed class Root:INCaptureState
    {
        public NCell<int> Cell=new(); public Root? Next;
        static readonly NCodeId Id=NCodeId.FromKey("probe.root.v1");
        public NCodeId CaptureSchemaId=>Id;
        public void Write(NCaptureWriter writer){writer.Field("cell",Cell);writer.Field("next",Next);}
        public void Read(NCaptureReader reader){Cell=reader.Field<NCell<int>>("cell");Next=reader.Field<Root?>("next");}
    }
}
