# .NET 10 & C# 14 — Feature Showcase

> **Engineering reference project** — practical, production-relevant demonstrations of C# 14 language features and .NET 10 platform improvements.

---

## Overview

This is **not** a beginner tutorial. Every feature demo shows:

- **Why the feature exists** — the real problem it solves
- **Before approach** — how it was done in C# ≤ 13 / .NET 9
- **After approach** — the new C# 14 / .NET 10 way
- **Working code** — compilable, runnable examples
- **Console output** — visible comparison when you run it
- **Performance notes** — where relevant, actual Stopwatch benchmarks

---

## Requirements

| Requirement | Version |
|---|---|
| .NET SDK | 10.0.x |
| C# Language | 14 (preview) |
| OS | Windows / Linux / macOS |

```bash
dotnet --version   # should print 10.0.x
```

---

## Quick Start

```bash
git clone https://github.com/Tejas-Chitroda/.NET10-C-14Demo.git
cd .NET10-C-14Demo/DotNet10Demo
dotnet run
```

An interactive console menu lets you run each feature individually or all at once.

---

## Project Structure

```
DotNet10Demo/
├── Program.cs                          ← Entry point + interactive menu
├── Utilities/
│   ├── ConsoleHelper.cs                ← Coloured output helpers
│   └── BenchmarkHelper.cs             ← Stopwatch + allocation measurement
└── Features/
    ├── FieldKeyword/
    │   └── FieldKeywordDemo.cs         ← C# 14: field keyword
    ├── ExtensionMembers/
    │   └── ExtensionMembersDemo.cs     ← C# 14: extension blocks
    ├── SpanImprovements/
    │   └── SpanImprovementsDemo.cs     ← Span<T> + implicit conversions
    ├── NullConditionalAssignment/
    │   └── NullConditionalDemo.cs      ← C# 14: obj?.Prop = value
    ├── UnboundGenerics/
    │   └── UnboundGenericsDemo.cs      ← C# 14: nameof(T<>)
    ├── PartialConstructors/
    │   ├── OrderAggregate.Generated.cs ← Simulated source-generator output
    │   ├── OrderAggregate.cs           ← Developer implementation
    │   └── PartialConstructorDemo.cs   ← C# 14: partial constructors
    ├── LinqImprovements/
    │   └── LinqImprovementsDemo.cs     ← .NET 10: LeftJoin / RightJoin
    ├── NativeAOT/
    │   └── NativeAOTDemo.cs           ← .NET 10: AOT + source-gen JSON
    └── FileBasedApps/
        ├── FileBasedAppsDemo.cs        ← .NET 10: dotnet run script.cs
        └── SampleScripts/
            ├── LogCleanup.cs           ← Real runnable file-based script
            └── EnvValidator.cs         ← Real runnable file-based script
```

---

## Features

### 1. `field` Keyword — C# 14

**File:** [Features/FieldKeyword/FieldKeywordDemo.cs](DotNet10Demo/Features/FieldKeyword/FieldKeywordDemo.cs)

Eliminates manual backing fields while preserving full validation and normalisation logic in property accessors.

```csharp
// BEFORE — manual backing field
private string _email = string.Empty;
public string Email {
    get => _email;
    set { _email = value.Trim().ToLowerInvariant(); }
}

// AFTER — C# 14 'field' keyword
public string Email {
    get;
    set { field = value.Trim().ToLowerInvariant(); }
} = string.Empty;
```

**Key benefits:**
- Removes all `_privateField` declarations
- Enables lazy-init caching directly in `get` accessor
- Works with property initialisers (`= defaultValue`)

**Run:** Menu option 1

---

### 2. Extension Members — C# 14

**File:** [Features/ExtensionMembers/ExtensionMembersDemo.cs](DotNet10Demo/Features/ExtensionMembers/ExtensionMembersDemo.cs)

New `extension` block syntax enables extension **properties**, **static members**, and **indexers** — not just methods.

```csharp
// BEFORE — methods only, awkward call-site syntax
public static decimal GetTotal(this Order o) => ...;
order.GetTotal()   // not order.Total

// AFTER — C# 14 extension block with properties
extension OrderExtensions(Order order)
{
    public decimal Subtotal => order.Lines.Sum(l => l.Quantity * l.UnitPrice);
    public decimal Total    => Subtotal + (Subtotal * 0.08m);
    public bool    IsValid  => !order.Lines.IsEmpty() && !order.IsCancelled;
    public static Order CreateForCustomer(string id, ...) => new() { ... };
}
order.Total   // feels native
```

**Run:** Menu option 2

---

### 3. Span\<T\> / ReadOnlySpan\<T\> Improvements — C# 14

**File:** [Features/SpanImprovements/SpanImprovementsDemo.cs](DotNet10Demo/Features/SpanImprovements/SpanImprovementsDemo.cs)

Implicit span conversions reduce boilerplate on hot paths. Zero-allocation CSV/log parsing benchmark included.

```csharp
// BEFORE — allocates string[] + N strings per row
var parts = line.Split(',');   // heap allocation every call

// AFTER — zero heap allocation
ReadOnlySpan<char> span = line;
var field = SliceNextField(ref span);   // window into original string
```

**Benchmark output example:**
```
⏱ String.Split approach                    142 µs  (baseline)
⏱ Span-based approach                        8 µs  (17.8x faster)
  String.Split allocated bytes:             248 bytes per call
  Span-based allocated bytes:                 0 bytes per call
```

**Run:** Menu option 3

---

### 4. Null-Conditional Assignment — C# 14

**File:** [Features/NullConditionalAssignment/NullConditionalDemo.cs](DotNet10Demo/Features/NullConditionalAssignment/NullConditionalDemo.cs)

`obj?.Property = value` — assignment is silently skipped when the target is null. RHS is also not evaluated.

```csharp
// BEFORE — nested null guards
if (customer.Address != null)
    if (patch.Street != null) customer.Address.Street = patch.Street;

// AFTER — C# 14
if (patch.Street != null) customer.Address?.Street = patch.Street;
```

**Important:** RHS expression (including side effects like method calls) is **not** evaluated when target is null.

**Run:** Menu option 4

---

### 5. Unbound Generic Types in `nameof` — C# 14

**File:** [Features/UnboundGenerics/UnboundGenericsDemo.cs](DotNet10Demo/Features/UnboundGenerics/UnboundGenericsDemo.cs)

`nameof(Repository<>)` now compiles — no dummy type argument required.

```csharp
// BEFORE — meaningless dummy type argument
string name = nameof(IRepository<object>);   // forced <object> just to compile

// AFTER — C# 14: unbound generic
string name = nameof(IRepository<>);         // clean, no dummy type
string name2 = nameof(IPipeline<,>);         // multi-arity too
```

**Run:** Menu option 5

---

### 6. Partial Constructors & Events — C# 14

**Files:**
- [Features/PartialConstructors/OrderAggregate.Generated.cs](DotNet10Demo/Features/PartialConstructors/OrderAggregate.Generated.cs) — simulated generator output
- [Features/PartialConstructors/OrderAggregate.cs](DotNet10Demo/Features/PartialConstructors/OrderAggregate.cs) — developer implementation
- [Features/PartialConstructors/PartialConstructorDemo.cs](DotNet10Demo/Features/PartialConstructors/PartialConstructorDemo.cs) — demo runner

```csharp
// Generated file declares the signature
public partial OrderAggregate(string customerId, decimal total);
public partial event EventHandler<OrderCreatedEventArgs>? OrderCreated;

// Developer file provides the body
public partial OrderAggregate(string customerId, decimal total)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
    Id = Guid.NewGuid().ToString("N")[..12];
    // ... domain logic
    RaiseOrderCreated(new OrderCreatedEventArgs(Id, CustomerId, Total));
}
```

**Run:** Menu option 6

---

### 7. LINQ: LeftJoin / RightJoin — .NET 10

**File:** [Features/LinqImprovements/LinqImprovementsDemo.cs](DotNet10Demo/Features/LinqImprovements/LinqImprovementsDemo.cs)

First-class LEFT JOIN and RIGHT JOIN operators replace the verbose `GroupJoin + SelectMany` pattern.

```csharp
// BEFORE — GroupJoin + SelectMany
customers.GroupJoin(orders, c => c.Id, o => o.CustomerId, (c, og) => (c, og))
    .SelectMany(x => x.og.DefaultIfEmpty(), (x, o) => (x.c, o));

// AFTER — .NET 10 LeftJoin
customers.LeftJoin(orders, c => c.Id, o => o.CustomerId, (c, o) => (c, o));
// o is Order? — null when no matching order
```

Also demonstrates: `CountBy`, `AggregateBy`, `Index()` (added in .NET 9, available in .NET 10).

**Run:** Menu option 7

---

### 8. NativeAOT Improvements — .NET 10

**File:** [Features/NativeAOT/NativeAOTDemo.cs](DotNet10Demo/Features/NativeAOT/NativeAOTDemo.cs)

Source-generated JSON serialisation (AOT-safe), publish instructions, and expected metrics.

```bash
# Publish as NativeAOT
dotnet publish -c Release -r win-x64 /p:PublishAot=true
```

| Metric | JIT (.NET 10) | NativeAOT (.NET 10) |
|---|---|---|
| Cold start | ~80-200ms | ~5-15ms |
| Binary size | ~100MB | ~8-15MB |
| Memory (idle) | ~35-50MB | ~8-20MB |

**Run:** Menu option 8

---

### 9. File-based Apps — .NET 10

**File:** [Features/FileBasedApps/FileBasedAppsDemo.cs](DotNet10Demo/Features/FileBasedApps/FileBasedAppsDemo.cs)

Run a single `.cs` file without a project file:

```bash
dotnet run Features/FileBasedApps/SampleScripts/LogCleanup.cs -- --days 30 --path /var/logs
dotnet run Features/FileBasedApps/SampleScripts/EnvValidator.cs -- --env staging
```

**Run:** Menu option 9

---

## Running Individual Sample Scripts

The file-based sample scripts are standalone and can be run directly:

```bash
# Log cleanup (dry-run)
dotnet run DotNet10Demo/Features/FileBasedApps/SampleScripts/LogCleanup.cs -- --dry-run --path .

# Environment validator
dotnet run DotNet10Demo/Features/FileBasedApps/SampleScripts/EnvValidator.cs -- --env development
```

---

## Performance Notes

| Feature | Impact |
|---|---|
| `field` keyword | Zero runtime cost — pure compile-time sugar |
| Extension members | Zero runtime cost — compiled to static methods |
| Span\<T\> CSV parsing | ~10-20x faster, zero allocations vs string.Split |
| NativeAOT | ~10-20x faster cold start, ~6x smaller binary |
| LeftJoin/RightJoin | Same perf as GroupJoin+SelectMany, far more readable |
| Source-gen JSON | ~20-30% faster than reflection-based serialiser |

---

## Common Mistakes

### `field` keyword
- **Mistake:** Using `field` outside a property accessor — it only works inside `get` / `set` / `init`.
- **Mistake:** Forgetting the `= default` initialiser — without it the field stays `default(T)`.

### Extension Members
- **Mistake:** Putting business logic in extensions — breaks encapsulation and discoverability.
- **Mistake:** Assuming extension properties are stored on the instance — they aren't; they're computed.

### Span\<T\>
- **Mistake:** Storing a `Span<T>` in a class field — `Span<T>` is a ref struct, stack-only.
- **Mistake:** Converting to `string` inside the hot loop — negates zero-allocation benefit.

### Null-Conditional Assignment
- **Mistake:** Expecting RHS side-effects when target is null — they are **not** executed.
- **Mistake:** Using in combination with `??=` — they have different semantics.

### NativeAOT
- **Mistake:** Using `JsonSerializer.Serialize(obj)` without a `JsonSerializerContext` — fails at runtime in AOT.
- **Mistake:** Using Castle/DynamicProxy or Moq — not AOT compatible.
- **Mistake:** Expecting EF Core lazy loading to work — it doesn't in NativeAOT.

---

## Architecture Notes

The project follows a deliberate structure:

- **One file per feature** — easy to copy-paste into your own codebase
- **Self-contained demos** — each `*Demo.Run()` method is independent
- **Before / After in same file** — compare old vs new approach side by side
- **Utilities are minimal** — only `ConsoleHelper` and `BenchmarkHelper`, nothing else

This is intentional. A showcase project should be a reference, not a framework.

---

## .NET Version Compatibility

| Feature | Min Version |
|---|---|
| `field` keyword | C# 14 / .NET 10 |
| Extension member blocks | C# 14 / .NET 10 (preview) |
| Null-conditional assignment | C# 14 / .NET 10 |
| `nameof(T<>)` unbound | C# 14 / .NET 10 |
| Partial constructors/events | C# 14 / .NET 10 |
| `LeftJoin` / `RightJoin` | .NET 10 |
| `CountBy` / `AggregateBy` | .NET 9+ |
| `Index()` | .NET 9+ |
| File-based apps | .NET 10 |
| NativeAOT (mature) | .NET 8+ (best in .NET 10) |

---

## License

MIT — use freely in production reference or internal tooling.
