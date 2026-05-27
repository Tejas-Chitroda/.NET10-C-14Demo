// =============================================================================
// FEATURE: Extension Members — new 'extension' block syntax (C# 14)
// =============================================================================
// WHY IT EXISTS:
//   Classic static extension methods work but have no concept of extension
//   PROPERTIES, STATIC MEMBERS, or INDEXERS on a type you don't own.
//   C# 14 introduces a first-class 'extension' block that can contain all
//   member kinds, bringing parity with instance members.
//
// PRODUCTION RELEVANCE:
//   - Enriching third-party types (HttpClient, IQueryable, ILogger)
//   - Domain-driven helpers that live next to the domain without coupling
//   - Pagination, sorting, filtering helpers on IEnumerable<T>
//   - Formatting / normalisation without polluting the core model
//
// NOTE: Extension member blocks require LangVersion=preview in .NET 10.
//       The runtime behaviour is identical to static extension methods.
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.ExtensionMembers;

// ─── Domain models ────────────────────────────────────────────────────────────
public record OrderLine(string ProductId, int Quantity, decimal UnitPrice);

public sealed class Order
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public string CustomerId { get; init; } = string.Empty;
    public List<OrderLine> Lines { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
}

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  Classic static extension method class (C# ≤ 13)
//   Methods only — no properties, no static extension factory
//   Renamed with "Get" prefix so they don't clash with the C# 14 extension block
// ─────────────────────────────────────────────────────────────────
public static class OrderExtensionsBefore
{
    // Properties must be expressed as Get-methods — awkward call-site syntax
    public static decimal GetSubtotal(this Order o) => o.Lines.Sum(l => l.Quantity * l.UnitPrice);
    public static decimal GetTax(this Order o) => GetSubtotal(o) * 0.08m;
    public static decimal GetTotal(this Order o) => GetSubtotal(o) + GetTax(o);
    public static bool GetIsEmpty(this Order o) => o.Lines.Count == 0;
    public static bool GetIsValid(this Order o) => !GetIsEmpty(o) && !o.IsCancelled;
    // No static factory — you can't add Order.Create(...) via old-style extension
}

public static class StringExtensionsBefore
{
    public static string NormalizeEmailOld(this string email) => email.Trim().ToLowerInvariant();
    public static bool CheckIsValidEmail(this string email)
    {
        var n = email.NormalizeEmailOld();
        return n.Contains('@') && n.LastIndexOf('.') > n.IndexOf('@');
    }
}

public static class EnumerableExtensionsBefore
{
    public static PagedResult<T> ToPagedList<T>(this IEnumerable<T> source, int page, int pageSize)
    {
        var list = source.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T>(items, list.Count, page, pageSize);
    }
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  C# 14 extension blocks: properties, static members,
//           indexers, and methods in one cohesive block
//
// IMPORTANT: extension blocks must live inside a top-level, non-generic
// static class. The class name is just a container — callers never reference it.
// ─────────────────────────────────────────────────────────────────

// Extension block for Order — adds computed PROPERTIES (not possible before)
// C# 14 syntax: extension(ReceiverType receiver) inside a static class container
public static class OrderExtensionBlock
{
    extension(Order order)
    {
        // Extension PROPERTIES — consume as order.Subtotal, order.Total etc.
        public decimal Subtotal => order.Lines.Sum(l => l.Quantity * l.UnitPrice);
        public decimal Tax => order.Subtotal * 0.08m;          // reference via receiver
        public decimal Total => order.Subtotal + order.Tax;
        public bool IsEmpty => order.Lines.Count == 0;
        public bool IsValid => !order.IsEmpty && !order.IsCancelled;

        // Extension METHOD still works inside the block
        public string Summarize() =>
            $"Order {order.Id} | {order.Lines.Count} lines | " +
            $"Subtotal {order.Subtotal:C} | Tax {order.Tax:C} | Total {order.Total:C}";

        // Static extension member — consume as Order.CreateForCustomer(...)
        public static Order CreateForCustomer(string customerId, params OrderLine[] lines) =>
            new() { CustomerId = customerId, Lines = [.. lines] };
    }
}

// Extension block for string — groups all string-helpers together
public static class EmailStringExtensionBlock
{
    extension(string value)
    {
        public string NormalizedEmail => value.Trim().ToLowerInvariant();
        public bool IsValidEmail =>
            value.NormalizedEmail.Contains('@') &&
            value.NormalizedEmail.LastIndexOf('.') > value.NormalizedEmail.IndexOf('@');
        public string EmailDomain =>
            value.IsValidEmail ? value.NormalizedEmail.Split('@')[1] : string.Empty;
    }
}

// Extension block for IEnumerable<T> with generic constraint
public static class EnumerablePageExtensionBlock
{
    extension<T>(IEnumerable<T> source)
    {
        public PagedResult<T> ToPaged(int page, int pageSize)
        {
            var list = source.ToList();
            var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<T>(items, list.Count, page, pageSize);
        }

        // Extension property: safe FirstOrDefault with a fallback
        public T? SafeFirst => source.FirstOrDefault();
    }
}

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class ExtensionMembersDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — Extension Members (new extension block syntax)");

        // ── Build sample data ─────────────────────────────────────
        var order = Order.CreateForCustomer("CUST-001",
            new OrderLine("SKU-A", 3, 19.99m),
            new OrderLine("SKU-B", 1, 149.00m),
            new OrderLine("SKU-C", 2, 8.50m));

        // ── BEFORE ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: Classic static extension methods");
        ConsoleHelper.WriteInfo("Properties must be method calls — GetSubtotal(), GetIsValid() etc.");
        ConsoleHelper.WriteBefore($"order.GetSubtotal() → {order.GetSubtotal():C}");
        ConsoleHelper.WriteBefore($"order.GetTotal()    → {order.GetTotal():C}");
        ConsoleHelper.WriteBefore($"order.GetIsValid()  → {order.GetIsValid()}");
        ConsoleHelper.WriteInfo("No static extension factory, no extension properties.");

        // ── AFTER ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: Extension block — properties feel native");
        ConsoleHelper.WriteAfter($"order.Subtotal  → {order.Subtotal:C}");
        ConsoleHelper.WriteAfter($"order.Tax       → {order.Tax:C}");
        ConsoleHelper.WriteAfter($"order.Total     → {order.Total:C}");
        ConsoleHelper.WriteAfter($"order.IsValid   → {order.IsValid}");
        ConsoleHelper.WriteAfter($"order.Summarize() → {order.Summarize()}");

        // ── String extensions ─────────────────────────────────────
        ConsoleHelper.WriteSection("String extension properties (email normalisation)");
        string raw = "  USER@Company.COM  ";
        ConsoleHelper.WriteResult("Raw input        ", raw.Trim());
        ConsoleHelper.WriteResult("NormalizedEmail  ", raw.NormalizedEmail);
        ConsoleHelper.WriteResult("IsValidEmail     ", raw.IsValidEmail);
        ConsoleHelper.WriteResult("EmailDomain      ", raw.EmailDomain);
        ConsoleHelper.WriteResult("'bad-email' IsValid", "bad-email".IsValidEmail);

        // ── Pagination extension ───────────────────────────────────
        ConsoleHelper.WriteSection("IEnumerable<T> pagination extension");
        var products = Enumerable.Range(1, 47).Select(i => $"Product-{i:D3}").ToList();
        var page2 = products.ToPaged(page: 2, pageSize: 10);
        ConsoleHelper.WriteResult("Total items      ", page2.TotalCount);
        ConsoleHelper.WriteResult("Page             ", page2.Page);
        ConsoleHelper.WriteResult("Total pages      ", page2.TotalPages);
        ConsoleHelper.WriteResult("Items on page 2  ", string.Join(", ", page2.Items));
        ConsoleHelper.WriteResult("Has next page    ", page2.HasNextPage);

        // ── Danger zone note ──────────────────────────────────────
        ConsoleHelper.WriteSection("Where extensions become dangerous");
        ConsoleHelper.WriteInfo("DO:   Add cross-cutting helpers (pagination, formatting, logging)");
        ConsoleHelper.WriteInfo("DO:   Extend third-party types you can't modify");
        ConsoleHelper.WriteInfo("DON'T: Put core domain logic in extensions — breaks encapsulation");
        ConsoleHelper.WriteInfo("DON'T: Use extensions to work around poor design");
        ConsoleHelper.WriteInfo("DON'T: Create extensions that modify mutable state unexpectedly");

        ConsoleHelper.WriteWhatChanged(
            "C# 14 extension blocks allow properties, static members, and indexers " +
            "alongside methods — all under one 'extension(Type x)' scope. This makes " +
            "enriching external types feel natural and keeps helpers tightly grouped.");
    }
}
