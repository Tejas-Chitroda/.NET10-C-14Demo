using System.Diagnostics;

namespace DotNet10Demo.Utilities;

/// <summary>
/// Lightweight benchmarking utility for comparing before/after approaches.
/// Uses Stopwatch with multiple iterations to reduce noise.
/// </summary>
public static class BenchmarkHelper
{
    /// <summary>
    /// Runs an action N times and returns elapsed microseconds (total / iterations).
    /// Includes a warm-up pass to avoid JIT-cold-start skew.
    /// </summary>
    public static long MeasureMicroseconds(Action action, int iterations = 100_000)
    {
        // Warm-up: one pass to trigger JIT compilation
        action();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            action();
        sw.Stop();

        // Return average microseconds per iteration
        return sw.ElapsedTicks * 1_000_000L / (Stopwatch.Frequency * iterations);
    }

    /// <summary>
    /// Runs two competing implementations and prints a side-by-side comparison.
    /// </summary>
    public static BenchmarkResult Compare(
        string beforeLabel,
        Action before,
        string afterLabel,
        Action after,
        int iterations = 100_000)
    {
        long beforeMicros = MeasureMicroseconds(before, iterations);
        long afterMicros  = MeasureMicroseconds(after,  iterations);

        double speedup = beforeMicros == 0 ? 1.0
            : (double)beforeMicros / afterMicros;

        return new BenchmarkResult(beforeLabel, beforeMicros, afterLabel, afterMicros, speedup);
    }

    /// <summary>
    /// Measures how many bytes are allocated on the heap per call.
    /// Uses GC.GetAllocatedBytesForCurrentThread for per-thread accuracy.
    /// </summary>
    public static long MeasureAllocatedBytes(Action action)
    {
        // Warm-up + GC quiesce
        action();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();

        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        long after = GC.GetAllocatedBytesForCurrentThread();

        return after - before;
    }
}

public record BenchmarkResult(
    string BeforeLabel, long BeforeMicros,
    string AfterLabel,  long AfterMicros,
    double Speedup)
{
    public void Print()
    {
        ConsoleHelper.WriteBenchmark(BeforeLabel, BeforeMicros, "baseline");
        ConsoleHelper.WriteBenchmark(AfterLabel,  AfterMicros,
            Speedup > 1.0 ? $"{Speedup:F1}x faster" : $"{1.0/Speedup:F1}x slower");
    }
}
