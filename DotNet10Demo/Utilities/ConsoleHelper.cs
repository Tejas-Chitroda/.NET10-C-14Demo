namespace DotNet10Demo.Utilities;

public static class ConsoleHelper
{
    public static void WriteHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(new string('═', 70));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('═', 70));
        Console.ResetColor();
    }

    public static void WriteSection(string label)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ── {label} ──");
        Console.ResetColor();
    }

    public static void WriteBefore(string description)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"  [BEFORE] {description}");
        Console.ResetColor();
    }

    public static void WriteAfter(string description)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [AFTER]  {description}");
        Console.ResetColor();
    }

    public static void WriteResult(string label, object value)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  {label}: ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(value);
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    public static void WriteBenchmark(string label, long microseconds, string? note = null)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"  ⏱ {label,-40} {microseconds,8} µs");
        if (note != null)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"  ({note})");
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void WriteSeparator()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {new string('─', 60)}");
        Console.ResetColor();
    }

    public static void WriteCode(string code)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        foreach (var line in code.Split('\n'))
            Console.WriteLine($"  │ {line}");
        Console.ResetColor();
    }

    public static void WriteWhatChanged(string explanation)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("  ◆ What Changed: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(explanation);
        Console.ResetColor();
    }

    public static bool PromptMenu(string[] options, out int choice)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ┌─ Select a Feature to Run ────────────────────────────────┐");
        for (int i = 0; i < options.Length; i++)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  │  ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{i + 1,2}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($". {options[i],-52}  │");
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  │   0. Exit{new string(' ', 53)}│");
        Console.WriteLine("  └──────────────────────────────────────────────────────────┘");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  Enter choice: ");
        Console.ResetColor();

        var input = Console.ReadLine();
        choice = int.TryParse(input, out var c) ? c : -1;
        return choice >= 0 && choice <= options.Length;
    }
}
