// =============================================================================
// FEATURE: File-based Apps — dotnet run script.cs  (.NET 10)
// =============================================================================
// WHAT:
//   Before .NET 10, every C# program needed a project:
//     dotnet new console -n MyTool  → creates .csproj, .sln, obj/, bin/
//   That's 5+ files just to run 10 lines of code.
//
//   .NET 10 lets you skip the project entirely:
//     dotnet run hello.cs
//   Just write a .cs file and run it. C# is now as quick as Python for scripts.
//
// OPTIONAL DIRECTIVES (add at top of the file):
//   #:package Newtonsoft.Json@13.0.3   ← add a NuGet package
//   #:framework net10.0                ← pick a target framework
// =============================================================================

using DotNet10Demo.Utilities;

namespace DotNet10Demo.Features.FileBasedApps;

public static class FileBasedAppsDemo
{
    public static void Run()
    {
        ConsoleHelper.WriteHeader(".NET 10 — File-based Apps  (dotnet run script.cs)");

        // ── BEFORE ────────────────────────────────────────────────
        ConsoleHelper.WriteSection("BEFORE: You needed a full project for any C# script");
        ConsoleHelper.WriteCode(
            "dotnet new console -n Greeter     ← creates project folder\n" +
            "cd Greeter\n" +
            "# edit Program.cs\n" +
            "dotnet run\n" +
            "# result: .csproj + .sln + obj/ + bin/ = 5+ files for 3 lines of code");
        ConsoleHelper.WriteBefore("Overhead: project scaffolding just to run a simple script");
        ConsoleHelper.WriteBefore("Teams often used Python/bash instead of C# for quick tasks");

        // ── AFTER ─────────────────────────────────────────────────
        ConsoleHelper.WriteSection("AFTER: .NET 10 — just write a .cs file and run it");
        ConsoleHelper.WriteCode(
            "dotnet run hello.cs               ← no project needed!");
        ConsoleHelper.WriteAfter("One file. No .csproj. No .sln. Full C# syntax.");

        // ── Simplest possible script ───────────────────────────────
        ConsoleHelper.WriteSection("Simplest example: hello.cs");
        ConsoleHelper.WriteCode(HelloScript);

        // ── Script with a NuGet package ───────────────────────────
        ConsoleHelper.WriteSection("Script that uses a NuGet package");
        ConsoleHelper.WriteCode(PackageScript);

        // ── Practical script: log cleanup ─────────────────────────
        ConsoleHelper.WriteSection("Practical script: delete old log files");
        ConsoleHelper.WriteCode(LogCleanupScript);

        // ── How to run ────────────────────────────────────────────
        ConsoleHelper.WriteSection("How to run file-based scripts");
        ConsoleHelper.WriteResult("Basic run  ", "dotnet run hello.cs");
        ConsoleHelper.WriteResult("With args  ", "dotnet run cleanup.cs -- --days 30");
        ConsoleHelper.WriteResult("Unix       ", "chmod +x hello.cs && ./hello.cs");

        // ── When to use / not use ─────────────────────────────────
        ConsoleHelper.WriteSection("When to use file-based apps");
        ConsoleHelper.WriteInfo("✓  Quick utility scripts (log cleanup, DB seed, env check)");
        ConsoleHelper.WriteInfo("✓  CI/CD pre-flight checks committed to the repo");
        ConsoleHelper.WriteInfo("✓  One-off data tasks that don't deserve a full project");
        ConsoleHelper.WriteInfo("✗  Production services → use a proper project");
        ConsoleHelper.WriteInfo("✗  Large multi-file apps → use a proper project");

        ConsoleHelper.WriteWhatChanged(
            ".NET 10 lets you run a single .cs file with 'dotnet run file.cs' — " +
            "no project file needed. Add #:package to pull in NuGet packages. " +
            "C# is now as quick to script with as Python or bash.");
    }

    private const string HelloScript =
        "// hello.cs — run with: dotnet run hello.cs\n\n" +
        "Console.WriteLine(\"Hello from a file-based C# script!\");\n" +
        "Console.WriteLine($\"Today is {DateTime.Today:D}\");";

    private const string PackageScript =
        "// greet.cs\n" +
        "#:package Spectre.Console@0.49.1   ← NuGet package, no .csproj needed\n\n" +
        "using Spectre.Console;\n\n" +
        "AnsiConsole.MarkupLine(\"[bold green]Hello[/] from [yellow]Spectre.Console[/]!\");";

    private const string LogCleanupScript =
        "// cleanup.cs — run with: dotnet run cleanup.cs -- --days 30\n" +
        "#:framework net10.0\n\n" +
        "int days   = args.Contains(\"--days\") ? int.Parse(args[Array.IndexOf(args, \"--days\") + 1]) : 7;\n" +
        "string dir = \".\";\n\n" +
        "var old = Directory.GetFiles(dir, \"*.log\")\n" +
        "    .Where(f => File.GetLastWriteTimeUtc(f) < DateTime.UtcNow.AddDays(-days))\n" +
        "    .ToList();\n\n" +
        "Console.WriteLine($\"Found {old.Count} log files older than {days} days.\");\n" +
        "old.ForEach(File.Delete);\n" +
        "Console.WriteLine(\"Done!\");";
}
