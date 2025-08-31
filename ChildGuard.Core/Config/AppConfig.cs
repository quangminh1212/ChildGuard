using System.Text.Json.Serialization;

namespace ChildGuard.Core.Config;

public class AppConfig
{
    public MonitoringConfig Monitoring { get; set; } = new();
    public ProtectionConfig Protection { get; set; } = new();
    public PolicyConfig Policy { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
}

public class MonitoringConfig
{
    public bool EnableKeyboardHook { get; set; } = true;
    public bool EnableMouseHook { get; set; } = true;
    public bool EnableActiveWindow { get; set; } = true;
    public bool EnableProcessWatcher { get; set; } = true;
    public bool EnableUsbWatcher { get; set; } = true;
}

public class ProtectionConfig
{
    public bool EnableBadWordsDetector { get; set; } = true;
    public bool EnableUrlSafetyChecker { get; set; } = true;
    public bool EnableAudioMonitor { get; set; } = false;
    public string? FfmpegPath { get; set; }
    public string[] BadWords { get; set; } = new[] { "bad", "xau" };
    public string[] UrlAllowList { get; set; } = Array.Empty<string>();
    public string[] UrlBlockList { get; set; } = Array.Empty<string>();
}

public class PolicyConfig
{
    public string QuietHoursStart { get; set; } = "21:30";
    public string QuietHoursEnd { get; set; } = "06:30";
    public List<QuietWindow> AdditionalQuietWindows { get; set; } = new();
    public List<string> BlockedProcesses { get; set; } = new();
    public List<string> AllowedProcessesDuringQuietHours { get; set; } = new();
    public List<PolicyRule> PolicyRules { get; set; } = new();
    public bool SoftEnforce { get; set; } = true;
    public int WarningCooldownSeconds { get; set; } = 60;
}

public class QuietWindow
{
    public DayOfWeek? Day { get; set; }
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
}

public class PolicyRule
{
    public string When { get; set; } = "*"; // cron-like or simple day/time pattern (future extension)
    public string Action { get; set; } = "block"; // block/allow/warn
    public string Target { get; set; } = "process:"; // process:<name>, url:<pattern>
}

public class LoggingConfig
{
    public int LogRetentionDays { get; set; } = 14;
    public int LogMaxSizeMB { get; set; } = 200;
}

