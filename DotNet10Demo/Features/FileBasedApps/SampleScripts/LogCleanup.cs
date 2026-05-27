#!/usr/bin/env dotnet-script
#:framework net10.0

// =============================================================================
// Standalone log cleanup script — run with:  dotnet run LogCleanup.cs
// This file has NO .csproj — it is a .NET 10 file-based app.
// =============================================================================

var days   = args.Contains("--days")  ? int.Parse(args[Array.IndexOf(args, "--days")  + 1]) : 30;
var path   = args.Contains("--path")  ? args[Array.IndexOf(args, "--path")  + 1] : ".";
var dryRun = args.Contains("--dry-run");
var cutoff = DateTime.UtcNow.AddDays(-days);

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"Log Cleanup Utility — .NET 10 file-based app");
Console.WriteLine($"Path: {Path.GetFullPath(path)}  |  Older than: {days} days  |  DryRun: {dryRun}");
Console.ResetColor();

if (!Directory.Exists(path))
{
    Console.Error.WriteLine($"Directory not found: {path}");
    Environment.Exit(1);
}

int count = 0;
long totalBytes = 0;

foreach (var file in Directory.GetFiles(path, "*.log", SearchOption.AllDirectories))
{
    var info = new FileInfo(file);
    if (info.LastWriteTimeUtc < cutoff)
    {
        totalBytes += info.Length;
        count++;
        Console.ForegroundColor = dryRun ? ConsoleColor.Yellow : ConsoleColor.DarkRed;
        Console.WriteLine($"  {(dryRun ? "[DRY-RUN]" : "[DELETED]")} {file}  ({info.Length / 1024.0:F1} KB)");
        Console.ResetColor();
        if (!dryRun) File.Delete(file);
    }
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\n  Done. {count} files, {totalBytes / 1024.0 / 1024.0:F2} MB freed.");
Console.ResetColor();
