using System.Diagnostics;

using ChildGuard.Core.Config;

namespace ChildGuard.Core.Policy;

public sealed class PolicyEngine
{
    private readonly ConfigManager _config;
    private readonly EnforcementManager _enforcement;
    private DateTime _lastWarned = DateTime.MinValue;

    public PolicyEngine(ConfigManager cfg, EnforcementManager enforcement)
    {
        _config = cfg;
        _enforcement = enforcement;
    }

    public bool IsQuietNow(DateTime? now = null)
    {
        var t = (now ?? DateTime.Now).TimeOfDay;
        var start = TimeSpan.Parse(_config.Current.Policy.QuietHoursStart);
        var end = TimeSpan.Parse(_config.Current.Policy.QuietHoursEnd);
        bool within = start <= end ? (t >= start && t <= end) : (t >= start || t <= end);
        if (within) return true;
        foreach (var w in _config.Current.Policy.AdditionalQuietWindows)
        {
            if (!TimeSpan.TryParse(w.Start, out var ws) || !TimeSpan.TryParse(w.End, out var we)) continue;
            bool sameDay = w.Day == null || w.Day == (now ?? DateTime.Now).DayOfWeek;
            if (!sameDay) continue;
            var within2 = ws <= we ? (t >= ws && t <= we) : (t >= ws || t <= we);
            if (within2) return true;
        }
        return false;
    }

    public bool ShouldBlockProcess(string processName)
    {
        if (_enforcement.IsTemporarilyAllowed(processName)) return false;
        var blocked = _config.Current.Policy.BlockedProcesses;
        var allowedDuring = _config.Current.Policy.AllowedProcessesDuringQuietHours;
        if (IsQuietNow() && allowedDuring.Contains(processName, StringComparer.OrdinalIgnoreCase)) return false;
        if (blocked.Contains(processName, StringComparer.OrdinalIgnoreCase)) return true;
        return false;
    }

    public bool CanWarn() => (DateTime.UtcNow - _lastWarned).TotalSeconds >= _config.Current.Policy.WarningCooldownSeconds;
    public void MarkWarned() => _lastWarned = DateTime.UtcNow;
}

