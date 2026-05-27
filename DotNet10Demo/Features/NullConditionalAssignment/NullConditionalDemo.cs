// =============================================================================
// FEATURE: Null-Conditional Assignment  (?. on left-hand side)  (C# 14)
// =============================================================================
// WHY IT EXISTS:
//   Before C# 14, updating a nullable optional sub-object required an if-check
//   before every assignment, cluttering DTO mapping, cache update, and
//   configuration-patch logic with repetitive null guards.
//
//   C# 14 allows:  obj?.Property = value;
//   The assignment is SKIPPED entirely when obj is null — no NullReferenceException,
//   no helper method, no if-block.
//
// PRODUCTION RELEVANCE:
//   - PATCH endpoint DTO → domain model mapping
//   - Optional audit / metadata objects on aggregate roots
//   - Cache entry metadata updates
//   - Configuration hot-reload patches
//   - Event enrichment in processing pipelines
//
// SIDE-EFFECT RULE:
//   The right-hand side expression is also NOT evaluated when the target is null.
//   Side effects (function calls, new objects) on the RHS are skipped.
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.NullConditionalAssignment;

// ─── Domain models ────────────────────────────────────────────────────────────
public sealed class Address
{
    public string Street  { get; set; } = string.Empty;
    public string City    { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class AuditInfo
{
    public DateTime? LastModifiedAt { get; set; }
    public string    ModifiedBy     { get; set; } = string.Empty;
    public int       Version        { get; set; }
}

public sealed class CacheMetadata
{
    public DateTime ExpiresAt  { get; set; }
    public string   ETag       { get; set; } = string.Empty;
    public bool     IsDirty    { get; set; }
}

public sealed class CustomerAggregate
{
    public string        Id       { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string        Name     { get; set; } = string.Empty;
    public string        Email    { get; set; } = string.Empty;
    public Address?      Address  { get; set; }         // optional — not all customers have it
    public AuditInfo?    Audit    { get; set; }         // optional — may not be tracked
    public CacheMetadata? Cache   { get; set; }
}

// PATCH DTO — only fields present in the HTTP PATCH body
public sealed class CustomerPatchRequest
{
    public string?  Name          { get; set; }
    public string?  Email         { get; set; }
    public string?  Street        { get; set; }
    public string?  City          { get; set; }
    public string?  Country       { get; set; }
    public string?  ModifiedBy    { get; set; }
}

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  Defensive if-null guards everywhere (C# ≤ 13)
// ─────────────────────────────────────────────────────────────────
public static class CustomerServiceBefore
{
    public static void ApplyPatch(CustomerAggregate customer, CustomerPatchRequest patch)
    {
        if (patch.Name  != null) customer.Name  = patch.Name;
        if (patch.Email != null) customer.Email = patch.Email;

        // Optional sub-object: Address — need explicit null guard before each field
        if (customer.Address != null)
        {
            if (patch.Street  != null) customer.Address.Street  = patch.Street;
            if (patch.City    != null) customer.Address.City    = patch.City;
            if (patch.Country != null) customer.Address.Country = patch.Country;
        }

        // Optional sub-object: AuditInfo
        if (customer.Audit != null)
        {
            if (patch.ModifiedBy != null)
            {
                customer.Audit.ModifiedBy     = patch.ModifiedBy;
                customer.Audit.LastModifiedAt = DateTime.UtcNow;
                customer.Audit.Version++;
            }
        }

        // Optional cache metadata — always bust when a PATCH arrives
        if (customer.Cache != null)
        {
            customer.Cache.IsDirty   = true;
            customer.Cache.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
        }
    }
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  Null-conditional assignment (C# 14)
// ─────────────────────────────────────────────────────────────────
public static class CustomerServiceAfter
{
    public static void ApplyPatch(CustomerAggregate customer, CustomerPatchRequest patch)
    {
        if (patch.Name  != null) customer.Name  = patch.Name;
        if (patch.Email != null) customer.Email = patch.Email;

        // Address sub-object — assignment is silently SKIPPED when Address is null
        if (patch.Street  != null) customer.Address?.Street  = patch.Street;
        if (patch.City    != null) customer.Address?.City    = patch.City;
        if (patch.Country != null) customer.Address?.Country = patch.Country;

        // Audit — silently skipped when customer.Audit is null
        if (patch.ModifiedBy != null)
        {
            customer.Audit?.ModifiedBy     = patch.ModifiedBy;
            customer.Audit?.LastModifiedAt = DateTime.UtcNow;  // RHS not evaluated if null
        }

        // Cache bust — one-liners with no if-blocks
        customer.Cache?.IsDirty   = true;
        customer.Cache?.ExpiresAt = DateTime.UtcNow.AddMinutes(5);
    }
}

// ─────────────────────────────────────────────────────────────────
// Side-effect demonstration: RHS is NOT evaluated when target is null
// ─────────────────────────────────────────────────────────────────
public static class SideEffectDemo
{
    private static int _callCount = 0;

    private static string ExpensiveCompute()
    {
        _callCount++;
        return $"computed-{Guid.NewGuid():N}";
    }

    public static void ShowSkippedExecution()
    {
        Address? addr = null;
        _callCount = 0;

        // RHS (ExpensiveCompute()) is NEVER called — target is null
        addr?.Street = ExpensiveCompute();
        int callsWhenNull = _callCount;   // stays 0

        addr = new Address();
        addr?.Street = ExpensiveCompute();
        int callsWhenNotNull = _callCount; // becomes 1

        ConsoleHelper.WriteResult("ExpensiveCompute calls when addr=null    ", callsWhenNull);
        ConsoleHelper.WriteResult("ExpensiveCompute calls when addr!=null   ", callsWhenNotNull);
        ConsoleHelper.WriteResult("addr.Street after assignment             ", addr!.Street);
    }
}

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class NullConditionalDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — Null-Conditional Assignment  (?. on LHS)");

        // ── Customer WITH optional sub-objects ────────────────────
        var customerFull = new CustomerAggregate
        {
            Name    = "Alice Smith",
            Email   = "alice@example.com",
            Address = new Address { Street = "10 Main St", City = "Boston", Country = "US" },
            Audit   = new AuditInfo { ModifiedBy = "system", Version = 1 },
            Cache   = new CacheMetadata { ExpiresAt = DateTime.UtcNow.AddHours(1), IsDirty = false }
        };

        // ── Customer WITHOUT optional sub-objects ─────────────────
        var customerMinimal = new CustomerAggregate
        {
            Name  = "Bob Jones",
            Email = "bob@example.com"
            // Address, Audit, Cache intentionally null
        };

        var patch = new CustomerPatchRequest
        {
            Name       = "Alice Smith-Updated",
            Street     = "20 Oak Ave",
            City       = "Cambridge",
            ModifiedBy = "admin-user"
        };

        // ── BEFORE ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: Explicit null guards (C# ≤ 13)");
        ConsoleHelper.WriteInfo("ApplyPatch requires nested if-blocks for every optional field.");
        CustomerServiceBefore.ApplyPatch(customerFull, patch);
        ConsoleHelper.WriteResult("Full customer name   ", customerFull.Name);
        ConsoleHelper.WriteResult("Full customer street ", customerFull.Address?.Street ?? "(null)");
        ConsoleHelper.WriteResult("Full customer audit  ", customerFull.Audit?.ModifiedBy ?? "(null)");
        ConsoleHelper.WriteResult("Cache dirty flag     ", customerFull.Cache?.IsDirty.ToString() ?? "(null)");
        ConsoleHelper.WriteSeparator();

        // Minimal customer — the if(customer.Address != null) blocks simply skip
        CustomerServiceBefore.ApplyPatch(customerMinimal, patch);
        ConsoleHelper.WriteResult("Minimal customer name  ", customerMinimal.Name);
        ConsoleHelper.WriteResult("Minimal customer street", customerMinimal.Address?.Street ?? "(null — skipped)");

        // Reset for AFTER demo
        customerFull.Name            = "Alice Smith";
        customerFull.Address!.Street = "10 Main St";
        customerFull.Address!.City   = "Boston";
        customerFull.Audit!.ModifiedBy = "system";
        customerFull.Audit!.Version    = 1;
        customerFull.Cache!.IsDirty    = false;
        customerMinimal.Name = "Bob Jones";

        // ── AFTER ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: Null-conditional assignment (C# 14)");
        ConsoleHelper.WriteInfo("Same logic: customer.Address?.Street = patch.Street;");
        ConsoleHelper.WriteInfo("If Address is null: assignment and RHS are both skipped.");
        CustomerServiceAfter.ApplyPatch(customerFull, patch);
        ConsoleHelper.WriteResult("Full customer name   ", customerFull.Name);
        ConsoleHelper.WriteResult("Full customer street ", customerFull.Address?.Street ?? "(null)");
        ConsoleHelper.WriteResult("Full customer audit  ", customerFull.Audit?.ModifiedBy ?? "(null)");
        ConsoleHelper.WriteResult("Cache dirty flag     ", customerFull.Cache?.IsDirty.ToString() ?? "(null)");
        ConsoleHelper.WriteSeparator();

        CustomerServiceAfter.ApplyPatch(customerMinimal, patch);
        ConsoleHelper.WriteResult("Minimal customer name  ", customerMinimal.Name);
        ConsoleHelper.WriteResult("Minimal customer street", customerMinimal.Address?.Street ?? "(null — skipped)");

        // ── Side-effect skipping ───────────────────────────────────
        ConsoleHelper.WriteSection("Side-effect skipping: RHS not evaluated when target is null");
        SideEffectDemo.ShowSkippedExecution();

        ConsoleHelper.WriteWhatChanged(
            "C# 14 allows '?.' on the LEFT-HAND side of assignments. When the " +
            "navigation target is null, both the assignment and the RHS expression " +
            "are skipped entirely — no NullReferenceException, no guard needed. " +
            "Reduces boilerplate in PATCH mapping, cache updates, and event enrichment.");
    }
}
