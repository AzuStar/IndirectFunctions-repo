using System.Threading.Channels;

namespace IndirectFunctions.SerializationExamples;

public static class ProducerConsumer
{
    public static async Task Run(string format)
    {
        var queue = Channel.CreateBounded<byte[]>(1);
        var consume = Consume(queue.Reader, format);
        await Produce(queue.Writer, format);
        await consume;
    }

    private static async Task Produce(ChannelWriter<byte[]> queue, string format)
    {
        using var producer = new Endpoint(); // Shared bootstrap.
        var job = new Job { Input = 4, Function = producer.Double };
        await queue.WriteAsync(Formats.Serialize(format, job));
        queue.Complete();
    }

    private static async Task Consume(ChannelReader<byte[]> queue, string format)
    {
        using var consumer = new Endpoint(); // Same bootstrap, independent context.
        await foreach (var bytes in queue.ReadAllAsync())
        {
            var job = Formats.Deserialize<Job>(format, bytes);
            int result = job.Function.Invoke(consumer.Context, job.Input);
            Program.Equal(8, result);
            Console.WriteLine($"PASS {format} producer/consumer -> {result}");
        }
    }
}
