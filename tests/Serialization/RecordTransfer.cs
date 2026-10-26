namespace IndirectFunctions.SerializationExamples;

public static class RecordTransfer
{
    public static byte[] Produce(Endpoint sender, string format)
    {
        var job = new Job { Input = 4, Function = sender.Double, Functions = [sender.Double, sender.Square] };
        var envelope = new Envelope(job with { Input = 5 }); // Copy record containing NFunc.
        Program.Equal(8, job.Function.Invoke(sender.Context, job.Input));
        Program.Equal(10, envelope.Job.Function.Invoke(sender.Context, envelope.Job.Input));
        return Formats.Serialize(format, envelope.Job);
    }

    public static void Consume(Endpoint receiver, string format, byte[] bytes)
    {
        var restored = Formats.Deserialize<Job>(format, bytes);
        Program.Equal(10, restored.Function.Invoke(receiver.Context, restored.Input));
        Program.Equal(2, restored.Functions.Count);
        Program.Equal(10, restored.Functions[0].Invoke(receiver.Context, restored.Input));
        Program.Equal(25, restored.Functions[1].Invoke(receiver.Context, restored.Input));
        Program.Equal(receiver.Double.CodeId, restored.Function.CodeId);
    }
}
