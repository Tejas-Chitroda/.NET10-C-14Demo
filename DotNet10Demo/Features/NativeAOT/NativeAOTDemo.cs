// =============================================================================
// FEATURE: NativeAOT — Ahead-of-Time compilation  (.NET 10)
// =============================================================================
// WHAT:
//   Normal .NET apps use a JIT (Just-In-Time) compiler:
//     - App starts → runtime loads → JIT compiles methods on first call
//     - Startup: ~100-300ms, binary needs the full .NET runtime (~100 MB)
//
//   NativeAOT compiles everything to native machine code AT PUBLISH TIME:
//     - No JIT, no runtime bundled
//     - Startup: ~5-15ms, binary is ~8-15 MB, self-contained
//
//   Great for: CLI tools, serverless functions, containers, microservices
//
// KEY RULE FOR .NET 10:
//   You cannot use reflection-based JSON serialization in NativeAOT.
//   Instead, use [JsonSerializable] to generate the serializer at build time.
// =============================================================================

using System.Diagnostics;
using System.Runtime.CompilerServices;
using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.NativeAOT;

// ─── AOT-safe JSON: use [JsonSerializable] instead of reflection ──────────────
// NativeAOT cannot use JsonSerializer with reflection.
// Add [JsonSerializable] and the compiler generates the serializer for you.

using System.Text.Json;
using System.Text.Json.Serialization;

// This one attribute replaces runtime reflection for these types
[JsonSerializable(typeof(PersonDto))]
[JsonSerializable(typeof(List<PersonDto>))]
public partial class AppJsonContext : JsonSerializerContext { }

public record PersonDto(string Name, int Age, string City);

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class NativeAOTDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader(".NET 10 — NativeAOT: Ahead-of-Time Compilation");

        // ── What is NativeAOT? ────────────────────────────────────
        ConsoleHelper.WriteSection("What is NativeAOT?");
        ConsoleHelper.WriteInfo("Normal .NET:    JIT compiles methods at runtime → slow startup");
        ConsoleHelper.WriteInfo("NativeAOT:      everything compiled at publish time → fast startup");
        ConsoleHelper.WriteSeparator();
        ConsoleHelper.WriteResult("Startup time  (normal JIT)", "~100–300ms");
        ConsoleHelper.WriteResult("Startup time  (NativeAOT) ", "~5–15ms");
        ConsoleHelper.WriteResult("Binary size   (normal JIT)", "~100 MB (needs .NET runtime)");
        ConsoleHelper.WriteResult("Binary size   (NativeAOT) ", "~8–15 MB (self-contained)");

        // ── Is this process running as NativeAOT? ────────────────
        ConsoleHelper.WriteSection("Is this process NativeAOT?");
        bool isAot = !RuntimeFeature.IsDynamicCodeSupported;
        ConsoleHelper.WriteResult("Running as NativeAOT", isAot ? "YES" : "NO (running as normal JIT)");
        ConsoleHelper.WriteInfo(isAot
            ? "You published with /p:PublishAot=true"
            : "Run 'dotnet publish -c Release -r win-x64 /p:PublishAot=true' to get NativeAOT");

        // ── JSON: BEFORE and AFTER ────────────────────────────────
        ConsoleHelper.WriteSection("JSON serialization: BEFORE vs AFTER");

        ConsoleHelper.WriteCode(
            "// BEFORE (reflection — breaks in NativeAOT):\n" +
            "var json = JsonSerializer.Serialize(person);   // ❌ NativeAOT incompatible\n\n" +
            "// AFTER (source-generated — AOT safe):\n" +
            "[JsonSerializable(typeof(PersonDto))]\n" +
            "public partial class AppJsonContext : JsonSerializerContext { }\n\n" +
            "var json = JsonSerializer.Serialize(person, AppJsonContext.Default.PersonDto);  // ✓");

        var person = new PersonDto("Alice", 30, "London");

        // AOT-safe serialization
        var json = JsonSerializer.Serialize(person, AppJsonContext.Default.PersonDto);
        ConsoleHelper.WriteAfter($"Serialized  : {json}");

        var back = JsonSerializer.Deserialize(json, AppJsonContext.Default.PersonDto);
        ConsoleHelper.WriteAfter($"Deserialized: Name={back?.Name}, Age={back?.Age}, City={back?.City}");

        // ── Speed comparison ──────────────────────────────────────
        ConsoleHelper.WriteSection("Speed benchmark: serialize 1000 people (10k iterations)");

        var people = Enumerable.Range(1, 1000)
            .Select(i => new PersonDto($"Person {i}", 20 + i % 50, "City" + i))
            .ToList();

        // AOT-safe path
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 10_000; i++)
            _ = JsonSerializer.Serialize(people, AppJsonContext.Default.ListPersonDto);
        sw.Stop();
        ConsoleHelper.WriteBenchmark("Source-gen serialize (AOT-safe)", sw.ElapsedMilliseconds, "ms total");

        // ── How to publish ────────────────────────────────────────
        ConsoleHelper.WriteSection("How to publish as NativeAOT");
        ConsoleHelper.WriteCode(
            "# Windows\n" +
            "dotnet publish -c Release -r win-x64 /p:PublishAot=true\n\n" +
            "# Linux\n" +
            "dotnet publish -c Release -r linux-x64 /p:PublishAot=true");

        // ── What works / what doesn't ─────────────────────────────
        ConsoleHelper.WriteSection("What works in NativeAOT");
        ConsoleHelper.WriteInfo("✓  Console apps, CLI tools");
        ConsoleHelper.WriteInfo("✓  ASP.NET Core Minimal APIs");
        ConsoleHelper.WriteInfo("✓  System.Text.Json with [JsonSerializable]");
        ConsoleHelper.WriteInfo("✓  HttpClient, gRPC");
        ConsoleHelper.WriteInfo("✗  Reflection.Emit (no runtime code generation)");
        ConsoleHelper.WriteInfo("✗  Dynamic proxies (Moq, Castle) — use source generators instead");
        ConsoleHelper.WriteInfo("✗  EF Core lazy loading — use eager loading (.Include())");

        ConsoleHelper.WriteWhatChanged(
            ".NET 10 NativeAOT: compiles your app to native code at publish time — " +
            "no JIT, no runtime bundled, ~5-15ms startup. Key requirement: use " +
            "[JsonSerializable] for JSON instead of reflection-based serialization.");
    }
}
