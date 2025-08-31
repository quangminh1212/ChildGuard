using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace ChildGuard.Core.Config;

public sealed class ConfigManager
{
    private readonly string _path;
    private FileSystemWatcher? _watcher;
    private readonly TimeSpan _debounce = TimeSpan.FromMilliseconds(500);
    private DateTime _lastSignal = DateTime.MinValue;
    private readonly object _sync = new();

    public AppConfig Current { get; private set; } = new();
    public event Action<AppConfig>? OnConfigChanged;

    public ConfigManager(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            Save(Current);
        }
        Load();
        StartWatch();
    }

    private void Load()
    {
        try
        {
            var json = File.ReadAllText(_path);
            var cfg = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            Current = cfg;
        }
        catch
        {
            // keep existing
        }
    }

    public void Save(AppConfig cfg)
    {
        var json = JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }

    private void StartWatch()
    {
        var dir = Path.GetDirectoryName(_path)!;
        var file = Path.GetFileName(_path);
        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes
        };
        _watcher.Changed += (_, __) => DebounceReload();
        _watcher.Created += (_, __) => DebounceReload();
        _watcher.Renamed += (_, __) => DebounceReload();
        _watcher.EnableRaisingEvents = true;
    }

    private void DebounceReload()
    {
        lock (_sync)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastSignal) < _debounce) return;
            _lastSignal = now;
            Task.Delay(_debounce).ContinueWith(_ =>
            {
                Load();
                OnConfigChanged?.Invoke(Current);
            });
        }
    }
}

