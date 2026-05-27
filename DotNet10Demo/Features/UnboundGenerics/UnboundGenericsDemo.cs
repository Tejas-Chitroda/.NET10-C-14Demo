// =============================================================================
// FEATURE: Unbound Generic Types in nameof  (C# 14)
// =============================================================================
// WHAT:  nameof() now accepts open/unbound generics:
//          nameof(List<>)       → "List"
//          nameof(Dictionary<,>) → "Dictionary"
//
// WHY:   Before C# 14, nameof() required a CLOSED generic (all type args filled).
//        If you just wanted the base name "Repository" for a log message, you
//        had to write  nameof(Repository<object>)  — the <object> is meaningless,
//        it's only there to satisfy the compiler.
//        C# 14 removes this annoyance.
//
// WHERE YOU USE THIS:
//   - Log messages:  $"[{nameof(Repository<>)}<{typeof(T).Name}>] ..."
//   - Exception messages referencing generic types
//   - DI registration helpers
//   - Any infrastructure code that works with generic types
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.UnboundGenerics;

// ─── Simple generic types used in the examples ────────────────────────────────
public class Repository<T>  { }
public class Pipeline<TIn, TOut> { }
public class Cache<T> { }

public record User(string Id, string Name);
public record Order(string Id, decimal Total);

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class UnboundGenericsDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — Unbound Generic Types in nameof");

        // ── BEFORE ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: Forced dummy type argument (C# ≤ 13)");
        ConsoleHelper.WriteInfo("nameof(Repository<>)   → COMPILE ERROR in C# ≤ 13");
        ConsoleHelper.WriteInfo("You had to write a meaningless type arg just to compile:");
        ConsoleHelper.WriteCode(
            "// Why <object>? No reason — just to satisfy the compiler.\n" +
            "string name = nameof(Repository<object>);   // returns \"Repository\"");

        // C# ≤ 13 style — dummy <object> required
        string beforeName1 = nameof(Repository<object>);          // <object> = meaningless
        string beforeName2 = nameof(Pipeline<object, object>);    // two dummy args
        string beforeName3 = nameof(Cache<object>);

        ConsoleHelper.WriteBefore($"nameof(Repository<object>)         = \"{beforeName1}\"  ← <object> is pointless");
        ConsoleHelper.WriteBefore($"nameof(Pipeline<object, object>)   = \"{beforeName2}\"  ← two pointless args");
        ConsoleHelper.WriteBefore($"nameof(Cache<object>)              = \"{beforeName3}\"  ← pointless arg");

        // ── AFTER ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: No dummy type args needed (C# 14)");
        ConsoleHelper.WriteCode(
            "string name = nameof(Repository<>);    // clean — no dummy type\n" +
            "string name = nameof(Pipeline<,>);     // two arity — still clean");

        // C# 14 — unbound generics, clean and correct
        string afterName1 = nameof(Repository<>);
        string afterName2 = nameof(Pipeline<,>);
        string afterName3 = nameof(Cache<>);

        ConsoleHelper.WriteAfter($"nameof(Repository<>)    = \"{afterName1}\"");
        ConsoleHelper.WriteAfter($"nameof(Pipeline<,>)     = \"{afterName2}\"");
        ConsoleHelper.WriteAfter($"nameof(Cache<>)         = \"{afterName3}\"");

        // ── Practical: log messages with generic type context ──────
        ConsoleHelper.WriteSection("Practical use: log/error messages with type info");
        ConsoleHelper.WriteCode(
            "// BEFORE (C# ≤ 13):\n" +
            "logger.LogError($\"[{nameof(Repository<object>)}<{typeof(T).Name}>] Save failed\");\n\n" +
            "// AFTER (C# 14):\n" +
            "logger.LogError($\"[{nameof(Repository<>)}<{typeof(T).Name}>] Save failed\");");

        string userError  = BuildErrorMessage<User>("GetById",  new TimeoutException("30s timeout"));
        string orderError = BuildErrorMessage<Order>("Save",    new InvalidOperationException("Duplicate key"));
        ConsoleHelper.WriteResult("User  error ", userError);
        ConsoleHelper.WriteResult("Order error ", orderError);

        // ── Practical: DI key generation ──────────────────────────
        ConsoleHelper.WriteSection("Practical use: DI service key generation");
        ConsoleHelper.WriteResult("Service key (User) ", BuildServiceKey<User>());
        ConsoleHelper.WriteResult("Service key (Order)", BuildServiceKey<Order>());
        ConsoleHelper.WriteResult("Cache key   (User) ", BuildCacheKey<User>());

        ConsoleHelper.WriteWhatChanged(
            "C# 14 lets nameof() accept open generics: nameof(List<>), nameof(Dict<,>). " +
            "No dummy type arguments. Cleaner log messages, error text, and DI helpers " +
            "that reference generic type names.");
    }

    // Helper methods — notice: nameof(Repository<>) not nameof(Repository<object>)
    private static string BuildErrorMessage<T>(string op, Exception ex) =>
        $"[{nameof(Repository<>)}<{typeof(T).Name}>] {op} failed: {ex.Message}";

    private static string BuildServiceKey<T>() =>
        $"{nameof(Repository<>)}_{typeof(T).Name}";

    private static string BuildCacheKey<T>() =>
        $"{nameof(Cache<>)}_{typeof(T).Name}";
}
