// =============================================================================
// FEATURE: LINQ Improvements — LeftJoin / RightJoin / CountBy / AggregateBy (.NET 10)
// =============================================================================
// WHY IT EXISTS:
//   GroupJoin + SelectMany was the only way to express LEFT JOIN in LINQ.
//   It works but produces verbose, hard-to-read code that looks nothing like SQL.
//   .NET 9 added CountBy / AggregateBy.
//   .NET 10 adds first-class LeftJoin / RightJoin operators.
//
// PRODUCTION RELEVANCE:
//   - In-memory join of domain collections before DB queries are built
//   - Joining DTOs from multiple microservice responses
//   - Report generation (customers without orders, products without stock)
//   - API aggregation layers that merge data from multiple sources
//
// NOTE: LeftJoin/RightJoin are in System.Linq namespace in .NET 10.
//       They generate IEnumerable<(TOuter, TInner?)> result tuples.
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.LinqImprovements;

// ─── Domain models ────────────────────────────────────────────────────────────
public record Customer(string Id, string Name, string Tier);
public record Order(string OrderId, string CustomerId, decimal Amount, string Status);
public record Product(string Id, string Name, decimal Price, string Category);
public record Inventory(string ProductId, int StockCount, string WarehouseId);

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  GroupJoin + SelectMany for LEFT JOIN (C# ≤ .NET 9)
// ─────────────────────────────────────────────────────────────────
public static class LinqBefore
{
    // LEFT JOIN: all customers, with their orders if any
    public static IEnumerable<(Customer Customer, Order? Order)>
        GetCustomersWithOrders(IEnumerable<Customer> customers, IEnumerable<Order> orders)
    {
        // Verbose GroupJoin + SelectMany pattern — hard to read at a glance
        return customers.GroupJoin(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, orderGroup) => (c, orderGroup))
            .SelectMany(
                x => x.orderGroup.DefaultIfEmpty(),
                (x, o) => (x.c, o));
    }

    // Custom CountBy workaround
    public static Dictionary<string, int> CountOrdersByStatus(IEnumerable<Order> orders)
    {
        return orders
            .GroupBy(o => o.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // Custom AggregateBy workaround
    public static Dictionary<string, decimal> RevenueByCustomer(IEnumerable<Order> orders)
    {
        return orders
            .GroupBy(o => o.CustomerId)
            .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));
    }
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  LeftJoin / RightJoin / CountBy / AggregateBy (.NET 10)
// ─────────────────────────────────────────────────────────────────
public static class LinqAfter
{
    // LEFT JOIN — reads exactly like SQL, one method call
    public static IEnumerable<(Customer Customer, Order? Order)>
        GetCustomersWithOrders(IEnumerable<Customer> customers, IEnumerable<Order> orders)
    {
        return customers.LeftJoin(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => (c, o));           // o is Order? — null when no match
    }

    // RIGHT JOIN — all orders, with their customer if found (e.g. orphan order detection)
    public static IEnumerable<(Customer? Customer, Order Order)>
        GetOrdersWithCustomers(IEnumerable<Customer> customers, IEnumerable<Order> orders)
    {
        return customers.RightJoin(
            orders,
            c => c.Id,
            o => o.CustomerId,
            (c, o) => (c, o));           // c is Customer? — null for orphan orders
    }

    // CountBy — .NET 9+: native grouping count without ToDictionary
    public static IEnumerable<KeyValuePair<string, int>>
        CountOrdersByStatus(IEnumerable<Order> orders) =>
        orders.CountBy(o => o.Status);

    // AggregateBy — .NET 9+: native aggregation per key
    public static IEnumerable<KeyValuePair<string, decimal>>
        RevenueByCustomer(IEnumerable<Order> orders) =>
        orders.AggregateBy(
            o => o.CustomerId,
            seed: 0m,
            (acc, o) => acc + o.Amount);

    // Index() — .NET 9+: adds position index without manual counter
    public static IEnumerable<(int Index, T Item)> WithIndex<T>(IEnumerable<T> source) =>
        source.Index();
}

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class LinqImprovementsDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader(".NET 10 — LINQ: LeftJoin / RightJoin / CountBy / AggregateBy");

        // ── Test data ─────────────────────────────────────────────
        var customers = new List<Customer>
        {
            new("C001", "Alice Corp",   "Gold"),
            new("C002", "Bob Ltd",      "Silver"),
            new("C003", "Carol Inc",    "Gold"),
            new("C004", "Dave Co",      "Bronze"),    // no orders — should appear with null Order
        };

        var orders = new List<Order>
        {
            new("O1", "C001", 1500.00m, "Shipped"),
            new("O2", "C001", 800.00m,  "Delivered"),
            new("O3", "C002", 250.00m,  "Pending"),
            new("O4", "C003", 3200.00m, "Shipped"),
            new("O5", "CXXX", 99.99m,   "Pending"),   // orphan — no customer match
        };

        // ── BEFORE: LEFT JOIN ─────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: LEFT JOIN via GroupJoin + SelectMany (C# ≤ .NET 9)");
        ConsoleHelper.WriteInfo("Verbose: customers.GroupJoin(...).SelectMany(x => x.orderGroup.DefaultIfEmpty(), ...)");
        var beforeResults = LinqBefore.GetCustomersWithOrders(customers, orders).ToList();
        PrintJoinResults(beforeResults);

        // ── AFTER: LEFT JOIN ──────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: LeftJoin (.NET 10) — clean, readable, SQL-like");
        ConsoleHelper.WriteInfo("Clean: customers.LeftJoin(orders, c => c.Id, o => o.CustomerId, ...)");
        var afterResults = LinqAfter.GetCustomersWithOrders(customers, orders).ToList();
        PrintJoinResults(afterResults);

        // ── RIGHT JOIN ────────────────────────────────────────────
        ConsoleHelper.WriteSection("RightJoin — orphan order detection (.NET 10)");
        ConsoleHelper.WriteInfo("Find orders that reference non-existent customers.");
        var rightResults = LinqAfter.GetOrdersWithCustomers(customers, orders).ToList();
        foreach (var (c, o) in rightResults)
        {
            if (c is null)
                ConsoleHelper.WriteError($"ORPHAN order {o.OrderId} for missing customer");
            else
                ConsoleHelper.WriteResult($"Order {o.OrderId}", $"Customer={c.Name}, Amount={o.Amount:C}");
        }

        // ── CountBy ───────────────────────────────────────────────
        ConsoleHelper.WriteSection("CountBy — orders by status (.NET 9+)");
        ConsoleHelper.WriteInfo("BEFORE: .GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count())");
        ConsoleHelper.WriteInfo("AFTER:  .CountBy(o => o.Status)");
        foreach (var kv in LinqAfter.CountOrdersByStatus(orders))
            ConsoleHelper.WriteResult($"  {kv.Key,-12}", $"{kv.Value} orders");

        // ── AggregateBy ───────────────────────────────────────────
        ConsoleHelper.WriteSection("AggregateBy — revenue by customer (.NET 9+)");
        ConsoleHelper.WriteInfo("BEFORE: .GroupBy(o => o.CustomerId).ToDictionary(g => g.Key, g => g.Sum(...))");
        ConsoleHelper.WriteInfo("AFTER:  .AggregateBy(o => o.CustomerId, seed: 0m, (acc, o) => acc + o.Amount)");
        foreach (var kv in LinqAfter.RevenueByCustomer(orders))
        {
            var name = customers.Find(c => c.Id == kv.Key)?.Name ?? kv.Key;
            ConsoleHelper.WriteResult($"  {name,-14}", $"{kv.Value:C}");
        }

        // ── Index() ───────────────────────────────────────────────
        ConsoleHelper.WriteSection("Index() — enumerate with position (.NET 9+)");
        ConsoleHelper.WriteInfo("BEFORE: Select((item, i) => (i, item))");
        ConsoleHelper.WriteInfo("AFTER:  .Index()");
        foreach (var (idx, c) in customers.Index())
            ConsoleHelper.WriteResult($"  [{idx}]", c.Name);

        ConsoleHelper.WriteWhatChanged(
            ".NET 10 adds LeftJoin/RightJoin as first-class LINQ operators — replacing " +
            "the verbose GroupJoin+SelectMany pattern. .NET 9 added CountBy/AggregateBy " +
            "and Index(). Together they close the gap between LINQ and SQL expressiveness.");
    }

    private static void PrintJoinResults(IEnumerable<(Customer Customer, Order? Order)> results)
    {
        foreach (var (c, o) in results)
        {
            if (o is null)
                ConsoleHelper.WriteResult($"  {c.Name,-14}", "(no orders)");
            else
                ConsoleHelper.WriteResult($"  {c.Name,-14}", $"Order {o.OrderId} = {o.Amount:C} [{o.Status}]");
        }
    }
}
