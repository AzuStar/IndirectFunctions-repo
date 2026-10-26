using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IndirectFunctions;

namespace IndirectFunctions.Comparison;

internal static class Program
{
    private const long Bias = 7;
    private const string ResultsStart = "<!-- indirect-functions-comparison-results:start -->";
    private const string ResultsEnd = "<!-- indirect-functions-comparison-results:end -->";
    private static long consumedChecksum;

    private static int Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            if (options.Help)
            {
                Console.WriteLine(Help);
                return 0;
            }
            if (options.Readme is not null)
                ValidateReadme(File.ReadAllText(Path.GetFullPath(options.Readme)));

            using var context = new NContext();
            Func<long, long> normal = options.Capture ? CreateNormalCaptured(Bias) : Work;
            NFunc<long, long> unmanaged = default;
            ManagedNFunc<long, long> managed = default;
            if (options.Profile == Profile.Unmanaged)
                unmanaged = options.Capture ? CreateUnmanagedCaptured(context, Bias) : CreateUnmanagedStateless(context);
            else
                managed = options.Capture ? CreateManagedCaptured(Bias) : CreateManagedStateless();

            Console.WriteLine($"Starting comparison: {options.Iterations:N0} calls per row, {options.Runs} paired runs, {options.Profile}, capture={options.Capture}.");
            Console.WriteLine("Progress updates about once per second; ETA refines as calls complete. Ctrl+C stops the run.");
            Func<long, long, Sample> measureNormal = (count, offset) => MeasureNormal(normal, count, offset);
            Func<long, long, Sample> measureIndirect = options.Profile == Profile.Unmanaged
                ? (count, offset) => MeasureUnmanaged(unmanaged, context, count, offset)
                : (count, offset) => MeasureManaged(managed, count, offset);
            var normalSamples = new Sample[options.Runs];
            var indirectSamples = new Sample[options.Runs];
            if (options.Warmup > 0)
            {
                var warmupProgress = new Progress(2.0 * options.Warmup);
                var normalWarmup = MeasureWithProgress("Warmup normal", options.Warmup, measureNormal, warmupProgress);
                var indirectWarmup = MeasureWithProgress("Warmup indirect", options.Warmup, measureIndirect, warmupProgress);
                ValidateChecksums(normalWarmup.Checksum, indirectWarmup.Checksum, "warmup");
                Consume(normalWarmup.Checksum);
                Consume(indirectWarmup.Checksum);
            }

            var progress = new Progress(2.0 * options.Iterations * options.Runs);
            for (int run = 0; run < options.Runs; run++)
            {
                string prefix = $"Run {run + 1}/{options.Runs}";
                // Alternate paired order without adding a branch inside either measured loop.
                if ((run & 1) == 0)
                {
                    normalSamples[run] = MeasureWithProgress(prefix + " normal", options.Iterations, measureNormal, progress);
                    indirectSamples[run] = MeasureWithProgress(prefix + " indirect", options.Iterations, measureIndirect, progress);
                }
                else
                {
                    indirectSamples[run] = MeasureWithProgress(prefix + " indirect", options.Iterations, measureIndirect, progress);
                    normalSamples[run] = MeasureWithProgress(prefix + " normal", options.Iterations, measureNormal, progress);
                }
                ValidateChecksums(normalSamples[run].Checksum, indirectSamples[run].Checksum, $"run {run + 1}");
                Consume(normalSamples[run].Checksum);
                Consume(indirectSamples[run].Checksum);
            }

            var report = Report(options, normalSamples, indirectSamples);
            Console.WriteLine(report);
            if (options.Readme is not null)
            {
                UpdateReadme(options.Readme, report);
                Console.WriteLine($"Updated comparison results: {Path.GetFullPath(options.Readme)}");
            }
            GC.KeepAlive(normal);
            GC.KeepAlive(context);
            GC.KeepAlive(consumedChecksum);
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Comparison error: {exception.Message}");
            return 2;
        }
    }

    private static long Work(long value) => unchecked(value * 17 + Bias);

    // No marker shares this lexical scope: baseline stays an ordinary compiler-generated closure.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Func<long, long> CreateNormalCaptured(long bias) => value => unchecked(value * 17 + bias);

    private static NFunc<long, long> CreateUnmanagedStateless(NContext context) =>
        NFunc<long, long>.Create(context, "benchmarks.comparison.unmanaged.stateless", Work);

    private static ManagedNFunc<long, long> CreateManagedStateless() =>
        ManagedNFunc<long, long>.Create("benchmarks.comparison.managed.stateless", Work);

    private static NFunc<long, long> CreateUnmanagedCaptured(NContext context, long bias) =>
        NFunc<long, long>.Create(context, "benchmarks.comparison.unmanaged.captured", value => unchecked(value * 17 + bias));

    private static ManagedNFunc<long, long> CreateManagedCaptured(long bias) =>
        ManagedNFunc<long, long>.Create("benchmarks.comparison.managed.captured", value => unchecked(value * 17 + bias));

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long LoopNormal(Func<long, long> callback, long iterations, long offset = 0)
    {
        long checksum = 0;
        long end = checked(offset + iterations);
        for (long iteration = offset; iteration < end; ++iteration)
            checksum = unchecked(checksum + callback(iteration & 1023));
        return checksum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long LoopUnmanaged(NFunc<long, long> callback, NContext context, long iterations, long offset = 0)
    {
        long checksum = 0;
        long end = checked(offset + iterations);
        for (long iteration = offset; iteration < end; ++iteration)
            checksum = unchecked(checksum + callback.Invoke(context, iteration & 1023));
        return checksum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long LoopManaged(ManagedNFunc<long, long> callback, long iterations, long offset = 0)
    {
        long checksum = 0;
        long end = checked(offset + iterations);
        for (long iteration = offset; iteration < end; ++iteration)
            checksum = unchecked(checksum + callback.Invoke(iteration & 1023));
        return checksum;
    }

    private static Sample MeasureNormal(Func<long, long> callback, long iterations, long offset)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        long checksum = LoopNormal(callback, iterations, offset);
        long end = Stopwatch.GetTimestamp();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        return new(end - start, allocated, checksum);
    }

    private static Sample MeasureUnmanaged(NFunc<long, long> callback, NContext context, long iterations, long offset)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        long checksum = LoopUnmanaged(callback, context, iterations, offset);
        long end = Stopwatch.GetTimestamp();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        return new(end - start, allocated, checksum);
    }

    private static Sample MeasureManaged(ManagedNFunc<long, long> callback, long iterations, long offset)
    {
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        long start = Stopwatch.GetTimestamp();
        long checksum = LoopManaged(callback, iterations, offset);
        long end = Stopwatch.GetTimestamp();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
        return new(end - start, allocated, checksum);
    }

    private static Sample MeasureWithProgress(string label, long iterations, Func<long, long, Sample> measure, Progress progress)
    {
        Console.WriteLine($"{label}: starting {iterations:N0} calls.");
        long completed = 0, ticks = 0, allocated = 0, checksum = 0;
        long chunkSize = 1_000_000;
        while (completed < iterations)
        {
            long count = Math.Min(chunkSize, iterations - completed);
            var sample = measure(count, completed);
            completed += count;
            ticks = checked(ticks + sample.Ticks);
            allocated = checked(allocated + sample.Allocated);
            checksum = unchecked(checksum + sample.Checksum);
            progress.Advance(label, count, completed, iterations);
            // Aim for 250 ms chunks. No logging, polling, or progress branches inside hot loops.
            if (sample.Ticks > 0)
                chunkSize = (long)Math.Clamp(count * (double)Stopwatch.Frequency / sample.Ticks / 4, 1_000, 100_000_000);
        }
        return new(ticks, allocated, checksum);
    }

    private sealed class Progress(double total)
    {
        private readonly long started = Stopwatch.GetTimestamp();
        private long lastUpdate = Stopwatch.GetTimestamp();
        private double completed;

        public void Advance(string label, long count, long rowCompleted, long rowTotal)
        {
            completed += count;
            long now = Stopwatch.GetTimestamp();
            if (rowCompleted != rowTotal && Stopwatch.GetElapsedTime(lastUpdate, now).TotalSeconds < 1)
                return;
            lastUpdate = now;
            double elapsed = Stopwatch.GetElapsedTime(started, now).TotalSeconds;
            double fraction = Math.Min(1, completed / total);
            double remaining = fraction >= 1 ? 0 : elapsed * (1 / fraction - 1);
            Console.WriteLine($"{label}: {rowCompleted:N0}/{rowTotal:N0} ({100.0 * rowCompleted / rowTotal:F1}%) | overall {100 * fraction:F1}% | elapsed {Duration(elapsed)} | ETA ~{Duration(remaining)}");
        }

        private static string Duration(double seconds)
        {
            if (!double.IsFinite(seconds)) return "estimating";
            if (seconds < 60) return $"{seconds:F1}s";
            if (seconds < 3600) return $"{Math.Floor(seconds / 60):F0}m {seconds % 60:F0}s";
            return $"{Math.Floor(seconds / 3600):F0}h {Math.Floor(seconds % 3600 / 60):F0}m";
        }
    }

    private static void ValidateChecksums(long normal, long indirect, string stage)
    {
        if (normal != indirect)
            throw new InvalidOperationException($"Checksum mismatch in {stage}: Func={normal}, indirect={indirect}.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Consume(long value) => Interlocked.Exchange(ref consumedChecksum, value);

    private static string Report(Options options, Sample[] normal, Sample[] indirect)
    {
        double normalTicks = Median(normal.Select(sample => (double)sample.Ticks));
        double indirectTicks = Median(indirect.Select(sample => (double)sample.Ticks));
        double pairedRatio = Median(normal.Zip(indirect, (first, second) =>
            first.Ticks == 0 ? double.NaN : (double)second.Ticks / first.Ticks));
        var profile = options.Profile == Profile.Unmanaged ? "NFunc<long, long> (unmanaged)" : "ManagedNFunc<long, long> (managed)";
        var report = new StringBuilder();
        report.AppendLine($"Run: {DateTimeOffset.UtcNow:O}; {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}; logical processors: {Environment.ProcessorCount}.");
        report.AppendLine();
        report.AppendLine($"Iterations per row/run: {options.Iterations.ToString(CultureInfo.InvariantCulture)}; paired runs: {options.Runs}; warmup calls per row: {options.Warmup.ToString(CultureInfo.InvariantCulture)}; capture: {options.Capture.ToString().ToLowerInvariant()}; Stopwatch frequency: {Stopwatch.Frequency.ToString(CultureInfo.InvariantCulture)} Hz.");
        report.AppendLine();
        report.AppendLine("| Callback | Median total (s) | ns/call | Ratio vs Func | Allocated bytes/call | Checksum |");
        report.AppendLine("|---|---:|---:|---:|---:|---:|");
        AddRow(report, "Func<long, long> (normal)", normalTicks, 1.0, Median(normal.Select(sample => (double)sample.Allocated)), normal[0].Checksum, options.Iterations);
        AddRow(report, profile, indirectTicks, pairedRatio, Median(indirect.Select(sample => (double)sample.Allocated)), indirect[0].Checksum, options.Iterations);
        report.AppendLine();
        report.Append("Ratio = median paired indirect/normal elapsed time. Timing sums adaptive chunks and excludes setup, binding, warmup, checksum validation, progress reporting, and output; checksums match each run. Allocation counts cover current-thread calls.");
        return report.ToString();
    }

    private static void AddRow(StringBuilder report, string name, double ticks, double ratio, double allocated, long checksum, long iterations)
    {
        double seconds = ticks / Stopwatch.Frequency;
        report.Append("| `").Append(name).Append("` | ").Append(Format(seconds, "F6"))
            .Append(" | ").Append(Format(seconds * 1_000_000_000 / iterations, "F3"))
            .Append(" | ").Append(Format(ratio, "F3"))
            .Append(" | ").Append(Format(allocated / iterations, "F6"))
            .Append(" | ").Append(checksum.ToString(CultureInfo.InvariantCulture)).AppendLine(" |");
    }

    private static string Format(double value, string format) => double.IsFinite(value)
        ? value.ToString(format, CultureInfo.InvariantCulture) : "n/a";

    private static double Median(IEnumerable<double> values)
    {
        var ordered = values.Order().ToArray();
        int middle = ordered.Length / 2;
        return (ordered.Length & 1) != 0 ? ordered[middle] : (ordered[middle - 1] + ordered[middle]) / 2;
    }

    private static (int Start, int End) ValidateReadme(string text)
    {
        int start = text.IndexOf(ResultsStart, StringComparison.Ordinal);
        int end = text.IndexOf(ResultsEnd, StringComparison.Ordinal);
        if (start < 0 || end < start + ResultsStart.Length
            || text.LastIndexOf(ResultsStart, StringComparison.Ordinal) != start
            || text.LastIndexOf(ResultsEnd, StringComparison.Ordinal) != end)
            throw new ArgumentException($"Results document must contain one ordered pair of {ResultsStart} and {ResultsEnd} markers.");
        return (start + ResultsStart.Length, end);
    }

    private static void UpdateReadme(string path, string report)
    {
        string fullPath = Path.GetFullPath(path);
        // Re-read after timing so unrelated edits made during a long run survive.
        string current = File.ReadAllText(fullPath);
        var bounds = ValidateReadme(current);
        string newline = current.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string replacement = report.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", newline, StringComparison.Ordinal);
        string next = current[..bounds.Start] + newline + replacement + newline + current[bounds.End..];
        string temporary = fullPath + ".comparison." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temporary, next, new UTF8Encoding(false));
            File.Move(temporary, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    private readonly record struct Sample(long Ticks, long Allocated, long Checksum);
    private enum Profile { Unmanaged, Managed }

    private sealed record Options(long Iterations, int Runs, long Warmup, bool Capture, Profile Profile, string? Readme, bool Help)
    {
        internal static Options Parse(string[] args)
        {
            long iterations = 100_000_000;
            int runs = 5;
            long warmup = 100_000;
            bool capture = false;
            Profile profile = Profile.Unmanaged;
            string? readme = null;
            bool help = false;
            for (int index = 0; index < args.Length; ++index)
            {
                string argument = args[index];
                int equals = argument.IndexOf('=');
                string option = equals < 0 ? argument : argument[..equals];
                string? inline = equals < 0 ? null : argument[(equals + 1)..];
                string Value()
                {
                    if (inline is not null)
                        return inline.Length == 0 ? throw new ArgumentException($"Missing value for {option}.") : inline;
                    if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                        throw new ArgumentException($"Missing value for {option}.");
                    return args[++index];
                }
                switch (option)
                {
                    case "--help" or "-h":
                        if (inline is not null)
                            throw new ArgumentException($"{option} does not accept a value.");
                        help = true;
                        break;
                    case "--iterations":
                        iterations = Count(Value(), option, allowZero: false);
                        break;
                    case "--runs":
                        long count = Count(Value(), option, allowZero: false);
                        runs = count <= int.MaxValue ? (int)count : throw new ArgumentException("--runs exceeds Int32.MaxValue.");
                        break;
                    case "--warmup":
                        warmup = Count(Value(), option, allowZero: true);
                        break;
                    case "--capture":
                        if (inline is not null || (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal)))
                        {
                            if (!bool.TryParse(Value(), out capture))
                                throw new ArgumentException("--capture accepts true or false.");
                        }
                        else
                            capture = true;
                        break;
                    case "--profile":
                        profile = Value().ToLowerInvariant() switch
                        {
                            "unmanaged" => Profile.Unmanaged,
                            "managed" => Profile.Managed,
                            _ => throw new ArgumentException("--profile accepts unmanaged or managed.")
                        };
                        break;
                    case "--readme":
                        readme = Value();
                        break;
                    default:
                        throw new ArgumentException($"Unknown option '{option}'. Use --help.");
                }
            }
            return new(iterations, runs, warmup, capture, profile, readme, help);
        }

        private static long Count(string text, string option, bool allowZero)
        {
            if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long value)
                || (allowZero ? value < 0 : value <= 0))
                throw new ArgumentException($"{option} requires a {(allowZero ? "nonnegative" : "positive")} Int64 value.");
            return value;
        }
    }

    private const string Help = """
        Compare normal Func<long, long> with one selected indirect profile.

        Usage: dotnet run --project benchmarks/Comparison -c Release -- [options]

          --iterations <Int64>  Calls per row/run; positive, default 100000000.
          --runs <Int32>        Paired runs; positive, default 5.
          --warmup <Int64>      Calls per row before timing; nonnegative, default 100000.
          --capture [bool]      Separate native/compiler and indirect captured closures; default false.
          --profile <name>      unmanaged (NFunc) or managed (ManagedNFunc); default unmanaged.
          --readme <path>       Replace bounded comparison results (use PERFORMANCE.md).
          --help, -h           Show usage; execute no callback or timing loops.

        Counts support 10 billion+ calls. Both loops use long counters and unchecked long checksums.
        Timing alternates row order, excludes setup/warmup/progress, and checks equal results each run.
        Progress includes row/overall percentage, elapsed time, and approximate ETA; updates about once per second.
        """;
}
