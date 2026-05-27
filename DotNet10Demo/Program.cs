using DotNet10Demo.Utilities;
using DotNet10Demo.Features.FieldKeyword;
using DotNet10Demo.Features.ExtensionMembers;
using DotNet10Demo.Features.SpanImprovements;
using DotNet10Demo.Features.NullConditionalAssignment;
using DotNet10Demo.Features.UnboundGenerics;
using DotNet10Demo.Features.PartialConstructors;
using DotNet10Demo.Features.LinqImprovements;
using DotNet10Demo.Features.NativeAOT;
using DotNet10Demo.Features.FileBasedApps;

// ─────────────────────────────────────────────────────────────────
//  .NET 10 & C# 14 Feature Showcase — Engineering Reference Project
// ─────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintBanner();

var features = new (string Label, Action Demo)[]
{
    ("field Keyword                 — C# 14",        FieldKeywordDemo.Run),
    ("Extension Members             — C# 14",        ExtensionMembersDemo.Run),
    ("Span<T> / ReadOnlySpan Improvements — C# 14",  SpanImprovementsDemo.Run),
    ("Null-Conditional Assignment   — C# 14",        NullConditionalDemo.Run),
    ("Unbound Generics in nameof    — C# 14",        UnboundGenericsDemo.Run),
    ("Partial Constructors & Events — C# 14",        PartialConstructorDemo.Run),
    ("LINQ: LeftJoin / RightJoin    — .NET 10",      LinqImprovementsDemo.Run),
    ("NativeAOT Improvements        — .NET 10",      NativeAOTDemo.Run),
    ("File-based Apps               — .NET 10",      FileBasedAppsDemo.Run),
    ("Run ALL features",                             RunAll),
};

bool running = true;
while (running)
{
    bool valid = ConsoleHelper.PromptMenu(features.Select(f => f.Label).ToArray(), out int choice);

    if (!valid || choice == 0)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n  Goodbye. Happy coding with .NET 10 & C# 14!\n");
        Console.ResetColor();
        running = false;
        continue;
    }

    Console.Clear();
    try
    {
        features[choice - 1].Demo();
    }
    catch (Exception ex)
    {
        ConsoleHelper.WriteError($"Demo threw an unexpected exception: {ex.Message}");
        ConsoleHelper.WriteInfo(ex.StackTrace ?? string.Empty);
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Press any key to return to menu...");
    Console.ResetColor();
    Console.ReadKey(intercept: true);
    Console.Clear();
    PrintBanner();
}

// ─── Local functions ──────────────────────────────────────────────
void RunAll()
{
    FieldKeywordDemo.Run();
    ExtensionMembersDemo.Run();
    SpanImprovementsDemo.Run();
    NullConditionalDemo.Run();
    UnboundGenericsDemo.Run();
    PartialConstructorDemo.Run();
    LinqImprovementsDemo.Run();
    NativeAOTDemo.Run();
    FileBasedAppsDemo.Run();
}

void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║          .NET 10  &  C# 14  —  Feature Showcase                ║");
    Console.WriteLine("  ╚══════════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"  Runtime: {Environment.Version}  |  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
    Console.ResetColor();
}
