// =============================================================================
// FILE: Product.Generated.cs  — simulates what a SOURCE GENERATOR emits
// =============================================================================
#nullable enable
// Imagine a generator that reads your [Entity] attribute and auto-creates:
//   • infrastructure properties (Id, CreatedAt)
//   • the constructor SIGNATURE  ← NEW in C# 14
//
// Before C# 14 the generator could NOT declare a constructor the developer
// completes.  Now it can.  The developer provides the BODY in Product.cs.
// =============================================================================

namespace DotNet10Demo.Features.PartialConstructors;

public partial class Product
{
    // ── GENERATED: infrastructure properties ──────────────────────
    public int      Id        { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // ── GENERATED: constructor SIGNATURE only (no body) ───────────
    // C# 14: generator declares it, developer writes the body in Product.cs
    public partial Product(string name, decimal price);

    // ── GENERATED: partial event declaration ──────────────────────
    // Developer wires the add/remove in Product.cs
    public partial event EventHandler<EventArgs>? OnCreated;
}
