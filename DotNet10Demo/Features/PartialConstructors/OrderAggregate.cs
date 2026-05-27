// =============================================================================
// FILE: Product.cs  — written by the DEVELOPER
// =============================================================================
// The developer owns:
//   • domain properties (Name, Price)
//   • the constructor BODY — validation + initialisation
//
// The generator owns the signature.  C# 14 merges both into ONE constructor.
// =============================================================================

namespace DotNet10Demo.Features.PartialConstructors;

public partial class Product
{
    // ── DEVELOPER: domain properties ──────────────────────────────
    public string  Name  { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    // ── DEVELOPER: constructor BODY ───────────────────────────────
    // C# 14: developer fills in the body declared by the generator
    public partial Product(string name, decimal price)
    {
        // Developer owns validation
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));

        // Developer sets domain fields
        Name  = name.Trim();
        Price = Math.Round(price, 2);

        // Generator-owned fields are also set here — one unified constructor
        Id        = Math.Abs(name.GetHashCode());
        CreatedAt = DateTime.UtcNow;
    }

    // ── Partial event (C# 14): generator declares, developer wires ─
    private EventHandler<EventArgs>? _onCreated;
    public partial event EventHandler<EventArgs>? OnCreated
    {
        add    => _onCreated += value;
        remove => _onCreated -= value;
    }

    public void RaiseCreated() => _onCreated?.Invoke(this, EventArgs.Empty);
}
