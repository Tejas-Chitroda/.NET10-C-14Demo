// =============================================================================
// FEATURE: field keyword (C# 14)
// =============================================================================
// WHY IT EXISTS:
//   Before C# 14, every validated/normalized property needed a manual backing
//   field, creating noisy boilerplate. The 'field' keyword lets the compiler
//   generate the backing field while still allowing custom getter/setter logic.
//
// PRODUCTION RELEVANCE:
//   - Domain models with invariant enforcement
//   - DTOs that normalise input on assignment
//   - Configuration objects with validated ranges
//   - Cleaner value-object implementations
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.FieldKeyword;

// ─────────────────────────────────────────────────────────────────
// BEFORE  ─  Manual backing fields (C# ≤ 13 style)
// ─────────────────────────────────────────────────────────────────
public sealed class UserBefore
{
    // Every validated property requires a manual backing field
    private string _email = string.Empty;
    private string _name  = string.Empty;
    private int    _age;
    private decimal _salary;

    public string Email
    {
        get => _email;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var trimmed = value.Trim().ToLowerInvariant();
            if (!trimmed.Contains('@'))
                throw new ArgumentException("Invalid email address.", nameof(value));
            _email = trimmed;
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var trimmed = value.Trim();
            if (trimmed.Length < 2 || trimmed.Length > 100)
                throw new ArgumentException("Name must be 2–100 characters.", nameof(value));
            _name = trimmed;
        }
    }

    public int Age
    {
        get => _age;
        set
        {
            if (value is < 18 or > 120)
                throw new ArgumentOutOfRangeException(nameof(value), "Age must be 18–120.");
            _age = value;
        }
    }

    public decimal Salary
    {
        get => _salary;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Salary cannot be negative.");
            _salary = Math.Round(value, 2);         // normalise to 2 decimal places
        }
    }

    public override string ToString() =>
        $"UserBefore {{ Name={Name}, Email={Email}, Age={Age}, Salary={Salary:C} }}";
}

// ─────────────────────────────────────────────────────────────────
// AFTER  ─  'field' keyword removes all backing fields (C# 14)
// ─────────────────────────────────────────────────────────────────
public sealed class UserAfter
{
    // No backing fields needed — 'field' refers to the compiler-generated one.
    // The auto-property getter still works: get; is sufficient.

    public string Email
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var trimmed = value.Trim().ToLowerInvariant();
            if (!trimmed.Contains('@'))
                throw new ArgumentException("Invalid email address.", nameof(value));
            field = trimmed;                        // 'field' == compiler-generated backing
        }
    } = string.Empty;

    public string Name
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            var trimmed = value.Trim();
            if (trimmed.Length < 2 || trimmed.Length > 100)
                throw new ArgumentException("Name must be 2–100 characters.", nameof(value));
            field = trimmed;
        }
    } = string.Empty;

    public int Age
    {
        get;
        set
        {
            if (value is < 18 or > 120)
                throw new ArgumentOutOfRangeException(nameof(value), "Age must be 18–120.");
            field = value;
        }
    }

    public decimal Salary
    {
        get;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Salary cannot be negative.");
            field = Math.Round(value, 2);
        }
    }

    public override string ToString() =>
        $"UserAfter  {{ Name={Name}, Email={Email}, Age={Age}, Salary={Salary:C} }}";
}

// ─────────────────────────────────────────────────────────────────
// Demo entry point
// ─────────────────────────────────────────────────────────────────
public static class FieldKeywordDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader("C# 14 — field Keyword");

        // ── Before ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: Manual backing fields (C# ≤ 13)");
        ConsoleHelper.WriteInfo("Each validated property requires a _privateField declaration.");
        ConsoleHelper.WriteInfo("4 properties → 4 backing fields → noisy, error-prone.");

        var before = new UserBefore();
        before.Name   = "  Alice Smith  ";   // leading/trailing spaces stripped
        before.Email  = "  ALICE@EXAMPLE.COM  ";
        before.Age    = 30;
        before.Salary = 75_000.5678m;
        ConsoleHelper.WriteResult("Result", before);

        // ── After ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: 'field' keyword (C# 14) — zero manual backing fields");
        ConsoleHelper.WriteInfo("Same validation/normalisation logic. Zero private fields.");

        var after = new UserAfter();
        after.Name   = "  Alice Smith  ";
        after.Email  = "  ALICE@EXAMPLE.COM  ";
        after.Age    = 30;
        after.Salary = 75_000.5678m;
        ConsoleHelper.WriteResult("Result", after);

        // ── Validation guards ─────────────────────────────────────
        ConsoleHelper.WriteSection("Validation guards fire correctly");
        TrySetInvalid(() => after.Age    = 15,       "Age = 15 (under 18)");
        TrySetInvalid(() => after.Email  = "not-email", "Email = 'not-email'");
        TrySetInvalid(() => after.Salary = -100m,    "Salary = -100");

        ConsoleHelper.WriteWhatChanged(
            "C# 14 'field' keyword accesses the compiler-generated backing field inside " +
            "a property accessor. Eliminates all manual _backing declarations while " +
            "keeping full validation/normalisation logic and lazy-init patterns.");
    }

    private static void TrySetInvalid(Action setter, string description)
    {
        try
        {
            setter();
            ConsoleHelper.WriteError($"Expected exception for: {description}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteSuccess($"Caught {ex.GetType().Name} for: {description}");
        }
    }
}
