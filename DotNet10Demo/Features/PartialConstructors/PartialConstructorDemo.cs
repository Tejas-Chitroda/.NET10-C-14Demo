// =============================================================================
// FEATURE: Partial Constructors & Partial Events  (C# 14)
// =============================================================================
// WHAT:  Split a constructor across two files — one declares the SIGNATURE,
//        the other writes the BODY.  Both compile into ONE constructor.
//
// WHY:   Source generators emit the "scaffold" file (Generated.cs).
//        Before C# 14, generators could NOT hook into the constructor —
//        the developer had to call a manual Init() method after new().
//        C# 14 lets the generator declare the constructor, so the developer
//        only fills in the body.  Forget Init()? The compiler stops you.
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.PartialConstructors;

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  Two-step construction (C# ≤ 13)
//   Generator creates an empty constructor.
//   Developer must remember to call Init() — nothing enforces it.
// ─────────────────────────────────────────────────────────────────
public class ProductBefore
{
    public string  Name  { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int     Id    { get; private set; }

    public ProductBefore() { }          // generator creates this

    // Developer must call this manually after new() — easy to forget!
    public void Init(string name, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Name  = name.Trim();
        Price = Math.Round(price, 2);
        Id    = Math.Abs(name.GetHashCode());
    }
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  Partial Constructor (C# 14)
//   See Product.Generated.cs → declares the signature
//   See Product.cs           → developer writes the body
//   Result: ONE constructor, compiler enforces both halves exist.
// ─────────────────────────────────────────────────────────────────
// (Product class lives in OrderAggregate.Generated.cs + OrderAggregate.cs)

public static class PartialConstructorDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — Partial Constructors & Partial Events");

        // ── BEFORE ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: Two-step construction (C# ≤ 13)");
        ConsoleHelper.WriteInfo("Generator gives you an empty constructor.");
        ConsoleHelper.WriteInfo("You must call Init() yourself — nothing stops you forgetting.");
        ConsoleHelper.WriteCode(
            "var p = new ProductBefore();   // compiles fine, object is broken!\n" +
            "// p.Name is empty, p.Id is 0  — Init() was never called\n" +
            "p.Init(\"Widget\", 9.99m);      // developer must remember this");

        // Demonstrate the problem: object exists but is uninitialised
        var broken = new ProductBefore();
        ConsoleHelper.WriteBefore($"After new() only → Name='{broken.Name}', Price={broken.Price}, Id={broken.Id}  ← all default!");

        broken.Init("Widget", 9.99m);
        ConsoleHelper.WriteBefore($"After Init()     → Name='{broken.Name}', Price={broken.Price}, Id={broken.Id}  ← now valid");

        // ── AFTER ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: Partial Constructor (C# 14)");
        ConsoleHelper.WriteInfo("Generator file (Product.Generated.cs):  public partial Product(string name, decimal price);");
        ConsoleHelper.WriteInfo("Developer file (Product.cs):            public partial Product(string name, decimal price) { ... }");
        ConsoleHelper.WriteInfo("Compiler merges both into ONE constructor.  No Init() needed.  Cannot forget.");
        ConsoleHelper.WriteCode(
            "var p = new Product(\"Widget\", 9.99m);  // one step, always valid");

        var p = new Product("Widget", 9.99m);
        ConsoleHelper.WriteAfter($"After new()  → Name='{p.Name}', Price={p.Price}, Id={p.Id}, CreatedAt={p.CreatedAt:HH:mm:ss}");

        // ── Partial event ──────────────────────────────────────────
        ConsoleHelper.WriteSection("Partial Event (C# 14)");
        ConsoleHelper.WriteInfo("Generator declares: public partial event EventHandler<EventArgs>? OnCreated;");
        ConsoleHelper.WriteInfo("Developer wires:    public partial event ... { add => ...; remove => ...; }");

        string eventLog = "(not fired)";
        p.OnCreated += (_, _) => eventLog = $"OnCreated fired for Product '{p.Name}'";
        p.RaiseCreated();
        ConsoleHelper.WriteAfter($"Event result → {eventLog}");

        // ── Validation still works ─────────────────────────────────
        ConsoleHelper.WriteSection("Validation in partial constructor body");
        TryCreate("",     9.99m, "empty name");
        TryCreate("Good", -1m,   "negative price");
        ConsoleHelper.WriteSuccess("Validation works exactly as if it were a normal constructor.");

        ConsoleHelper.WriteWhatChanged(
            "C# 14 partial constructors: one file declares the signature, another " +
            "provides the body — compiled into a single constructor. Source generators " +
            "can now participate in construction without forcing Init() workarounds.");
    }

    private static void TryCreate(string name, decimal price, string scenario)
    {
        try   { _ = new Product(name, price); ConsoleHelper.WriteError($"Expected exception for: {scenario}"); }
        catch (Exception ex) { ConsoleHelper.WriteSuccess($"Caught {ex.GetType().Name} for '{scenario}'"); }
    }
}
