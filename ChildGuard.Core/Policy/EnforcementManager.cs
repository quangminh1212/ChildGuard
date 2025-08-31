using System.Collections.Concurrent;

namespace ChildGuard.Core.Policy;

public sealed class EnforcementManager
{
    private readonly ConcurrentDictionary<string, DateTime> _allowUntil = new(StringComparer.OrdinalIgnoreCase);

    public void Snooze(string processName, int minutes)
    {
        var until = DateTime.UtcNow.AddMinutes(minutes);
        _allowUntil.AddOrUpdate(processName, until, (_, __) => until);
        Cleanup();
    }

    public bool IsTemporarilyAllowed(string processName)
    {
        if (_allowUntil.TryGetValue(processName, out var until))
        {
            if (until > DateTime.UtcNow) return true;
            _allowUntil.TryRemove(processName, out _);
        }
        return false;
    }

    private void Cleanup()
    {
        foreach (var kv in _allowUntil)
        {
            if (kv.Value <= DateTime.UtcNow)
            {
                _allowUntil.TryRemove(kv.Key, out _);
            }
        }
    }
}

