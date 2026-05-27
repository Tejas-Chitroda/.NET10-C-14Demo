// =============================================================================
// FEATURE: Span<T> / ReadOnlySpan<T>  (C# 14)
// =============================================================================
//
// WHAT IS A SPAN?
//   A Span is like a "window" or "view" into an existing string or array.
//   It does NOT copy the data — it just points to a piece of it.
//
//   Think of it like highlighting text in a Word document:
//     • string.Split  = physically cutting the paper → creates new pieces
//     • Span           = highlighting the word      → nothing new created
//
// SIMPLE EXAMPLE:
//   text  =  "Alice,30,London"
//   Goal  =  get the name part → "Alice"
//
//   BEFORE (string.Split):
//     text.Split(',')[0]          → creates string[] + "Alice" + "30" + "London"
//                                    = 4 new objects on the heap
//
//   AFTER (ReadOnlySpan):
//     text.AsSpan()[..firstComma] → just a pointer+length into "Alice,30,London"
//                                    = 0 new objects on the heap
//
// C# 14 BONUS:
//   Before C# 14 you had to write:  GetName(text.AsSpan())
//   C# 14 converts automatically:   GetName(text)   ← cleaner!
//
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.SpanImprovements;

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  string.Split  (creates new string objects)
// ─────────────────────────────────────────────────────────────────
public static class NameParserBefore
{
    // Given "Alice,30,London" → returns "Alice"
    // string.Split creates: string[]  +  "Alice"  +  "30"  +  "London"
    // That's 4 new heap objects for every single call.
    public static string GetName(string csv)
    {
        string[] parts = csv.Split(',');   // ← new string[] allocated
        return parts[0];                   // ← "Alice" is a new string
    }

    public static string GetCity(string csv)
    {
        string[] parts = csv.Split(',');   // ← another allocation
        return parts[2];                   // ← "London" is a new string
    }
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  ReadOnlySpan<char>  (zero new objects)
// ─────────────────────────────────────────────────────────────────
public static class NameParserAfter
{
    // Same goal: given "Alice,30,London" → returns the "Alice" part
    // No new string created — the span is just a window into the original.
    public static ReadOnlySpan<char> GetName(ReadOnlySpan<char> csv)
    {
        int comma = csv.IndexOf(',');      // find the first comma
        return csv[..comma];              // slice from 0 to the comma → "Alice"
        //                                  ↑ NO copy, NO new string
    }

    public static ReadOnlySpan<char> GetCity(ReadOnlySpan<char> csv)
    {
        int first  = csv.IndexOf(',');
        int second = csv[(first + 1)..].IndexOf(',') + first + 1;
        return csv[(second + 1)..];       // everything after the second comma
    }
}

// ─────────────────────────────────────────────────────────────────
// C# 14: Implicit conversion — string → ReadOnlySpan<char>
//
// Before C# 14:  GetName(text.AsSpan())   ← had to call .AsSpan() manually
// After  C# 14:  GetName(text)            ← string converts automatically!
// ─────────────────────────────────────────────────────────────────
public static class ImplicitConversionDemo
{
    // This method accepts ReadOnlySpan<char>
    public static bool StartsWithHello(ReadOnlySpan<char> text) =>
        text.StartsWith("Hello", StringComparison.OrdinalIgnoreCase);
}

// ─────────────────────────────────────────────────────────────────
// Allocation measurement helper
// ─────────────────────────────────────────────────────────────────
file static class AllocationMeasurer
{
    internal static long Measure(Action action)
    {
        action(); // warm-up
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class SpanImprovementsDemo
{
    private const string SampleCsv = "Alice,30,London";

    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — Span<T> / ReadOnlySpan<T>");

        // ── What is a Span? ───────────────────────────────────────
        ConsoleHelper.WriteSection("What is a Span?");
        ConsoleHelper.WriteInfo("A Span is a 'window' into an existing string or array.");
        ConsoleHelper.WriteInfo("It does NOT copy data — it just points to a slice of it.");
        ConsoleHelper.WriteInfo($"Input: \"{SampleCsv}\"  →  Goal: extract the Name field");

        // ── BEFORE: string.Split ──────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: string.Split (C# ≤ 13)");
        ConsoleHelper.WriteCode(
            "string[] parts = csv.Split(',');   // creates string[] + 3 new strings\n" +
            "return parts[0];                   // \"Alice\" is a brand-new string object");

        string beforeName = NameParserBefore.GetName(SampleCsv);
        string beforeCity = NameParserBefore.GetCity(SampleCsv);
        ConsoleHelper.WriteBefore($"Name = \"{beforeName}\"  ← new string created");
        ConsoleHelper.WriteBefore($"City = \"{beforeCity}\"  ← new string created");

        // ── AFTER: ReadOnlySpan ───────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: ReadOnlySpan<char> (zero new objects)");
        ConsoleHelper.WriteCode(
            "int comma = csv.IndexOf(',');      // find where to slice\n" +
            "return csv[..comma];               // window into original — NO copy!");

        ReadOnlySpan<char> afterName = NameParserAfter.GetName(SampleCsv);
        ReadOnlySpan<char> afterCity = NameParserAfter.GetCity(SampleCsv);
        ConsoleHelper.WriteAfter($"Name = \"{afterName.ToString()}\"  ← no new string, just a view");
        ConsoleHelper.WriteAfter($"City = \"{afterCity.ToString()}\"  ← no new string, just a view");

        // ── C# 14: Implicit conversion ────────────────────────────
        ConsoleHelper.WriteSection("C# 14: Implicit string → ReadOnlySpan<char>");
        ConsoleHelper.WriteCode(
            "// BEFORE C# 14: had to call .AsSpan() explicitly\n" +
            "bool ok = StartsWithHello(text.AsSpan());   // .AsSpan() required\n\n" +
            "// C# 14: string converts automatically — no .AsSpan() needed!\n" +
            "bool ok = StartsWithHello(text);            // clean!");

        string greeting = "Hello, world!";
        string other    = "Goodbye!";

        // C# 14: passing plain string where ReadOnlySpan<char> is expected — works!
        ConsoleHelper.WriteAfter($"StartsWithHello(\"{greeting}\") = {ImplicitConversionDemo.StartsWithHello(greeting)}");
        ConsoleHelper.WriteAfter($"StartsWithHello(\"{other}\")    = {ImplicitConversionDemo.StartsWithHello(other)}");

        // ── Allocation comparison ─────────────────────────────────
        ConsoleHelper.WriteSection("How much memory does each approach allocate?");
        ConsoleHelper.WriteInfo("Measured using GC.GetAllocatedBytesForCurrentThread()");

        long beforeBytes = AllocationMeasurer.Measure(() => NameParserBefore.GetName(SampleCsv));
        long afterBytes  = AllocationMeasurer.Measure(() => { NameParserAfter.GetName(SampleCsv); });

        ConsoleHelper.WriteBenchmark("string.Split — bytes allocated", beforeBytes, "string[] + new strings");
        ConsoleHelper.WriteBenchmark("Span slice   — bytes allocated", afterBytes,  "zero (just a pointer)");

        // ── Speed comparison ──────────────────────────────────────
        ConsoleHelper.WriteSection("Speed comparison (200k iterations)");
        var bench = BenchmarkHelper.Compare(
            "string.Split",
            () => NameParserBefore.GetName(SampleCsv),
            "Span slice",
            () => { NameParserAfter.GetName(SampleCsv); },
            iterations: 200_000);
        bench.Print();

        ConsoleHelper.WriteWhatChanged(
            "ReadOnlySpan<char> gives you a zero-allocation 'window' into a string. " +
            "C# 14 makes it easier by letting you pass a plain string where " +
            "ReadOnlySpan<char> is expected — no .AsSpan() call needed.");
    }
}

