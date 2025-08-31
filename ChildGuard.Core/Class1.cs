using System;
using System.IO;

namespace ChildGuard.Core;

public static class Paths
{
    public static string AppDataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChildGuard");
    public static string LogsDir => Path.Combine(AppDataRoot, "logs");
    public static string ConfigFile => Path.Combine(AppDataRoot, "config.json");
}
