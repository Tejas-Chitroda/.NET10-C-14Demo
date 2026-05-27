#!/usr/bin/env dotnet-script
#:framework net10.0

// =============================================================================
// Pre-deployment environment validator — run with:  dotnet run EnvValidator.cs
// Use in CI/CD pipelines to ensure all required env vars are set before deploy.
// =============================================================================

string env = args.Contains("--env") ? args[Array.IndexOf(args, "--env") + 1] : "development";

// Define requirements per environment
var requirements = new Dictionary<string, string[]>
{
    ["development"] = ["DATABASE_URL", "REDIS_URL"],
    ["staging"]     = ["DATABASE_URL", "REDIS_URL", "JWT_SECRET", "API_KEY"],
    ["production"]  = ["DATABASE_URL", "REDIS_URL", "JWT_SECRET", "API_KEY", "SMTP_HOST", "SENTRY_DSN"],
};

if (!requirements.TryGetValue(env, out var required))
{
    Console.Error.WriteLine($"Unknown environment: {env}");
    Environment.Exit(2);
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"\n  Environment Validator — target: {env.ToUpperInvariant()}");
Console.WriteLine($"  Checking {required.Length} required variables...\n");
Console.ResetColor();

bool allOk = true;
foreach (var key in required)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrEmpty(value))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ MISSING  {key}");
        Console.ResetColor();
        allOk = false;
    }
    else
    {
        string masked = value.Length > 4
            ? value[..4] + new string('*', Math.Min(value.Length - 4, 8))
            : "****";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ OK       {key,-20} = {masked}");
        Console.ResetColor();
    }
}

Console.WriteLine();
if (allOk)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  All checks passed. Safe to deploy to {env}.");
    Console.ResetColor();
    Environment.Exit(0);
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  Validation FAILED. Do NOT deploy to {env}.");
    Console.ResetColor();
    Environment.Exit(1);
}
