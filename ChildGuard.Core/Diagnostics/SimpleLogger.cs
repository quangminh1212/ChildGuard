using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ChildGuard.Core.Diagnostics;

public static class SimpleLogger
{
    private static readonly object _gate = new();
    private static string _baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChildGuard", "logs");
    private static string _currentLogPath = BuildLogPath();
    private static DateTime _currentDate = DateTime.UtcNow.Date;

    public static void SetBaseDirectory(string baseDir)
    {
        if (string.IsNullOrWhiteSpace(baseDir)) return;
        lock (_gate)
        {
            _baseDir = baseDir;
            _currentLogPath = BuildLogPath();
        }
    }

    public static void Info(string message, params object[] args) => Write("INFO", message, args);
    public static void Debug(string message, params object[] args) => Write("DEBUG", message, args);
    public static void Error(string message, params object[] args) => Write("ERROR", message, args);
    public static void Error(Exception ex, string message = "", params object[] args)
    {
        var suffix = string.IsNullOrWhiteSpace(message) ? string.Empty : (" " + string.Format(message, args));
        Write("ERROR", $"{ex.GetType().Name}: {ex.Message}{suffix}\n{ex.StackTrace}");
    }

    private static void Write(string level, string message, params object[] args)
    {
        try
        {
            var now = DateTime.UtcNow;
            if (now.Date != _currentDate)
            {
                lock (_gate)
                {
                    _currentDate = now.Date;
                    _currentLogPath = BuildLogPath();
                }
            }

            var line = $"{now:O} [{level}] {Format(message, args)}";
            // Debug output for dev
            System.Diagnostics.Debug.WriteLine(line);

            // Console when available
            try { Console.WriteLine(line); } catch { }

            // File
            lock (_gate)
            {
                Directory.CreateDirectory(_baseDir);
                File.AppendAllText(_currentLogPath, line + Environment.NewLine);
            }
        }
        catch { /* swallow logging failures */ }
    }

    private static string Format(string message, params object[] args)
    {
        try { return args is { Length: > 0 } ? string.Format(message, args) : message; }
        catch { return message; }
    }

    private static string BuildLogPath()
    {
        var proc = GetSafeProcessName();
        return Path.Combine(_baseDir, $"{proc}-{DateTime.UtcNow:yyyyMMdd}.log");
    }

    private static string GetSafeProcessName()
    {
        try
        {
            var name = Process.GetCurrentProcess().ProcessName;
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
        catch { return "ChildGuard"; }
    }
}

