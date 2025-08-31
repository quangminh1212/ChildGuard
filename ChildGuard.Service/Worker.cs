using ChildGuard.Core.Config;
using ChildGuard.Core.Logging;
using ChildGuard.Core.Monitoring;
using ChildGuard.Core.Policy;

namespace ChildGuard.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ConfigManager _config;
    private readonly JsonlLogger _jsonl;
    private readonly HookManager _hooks;
    private readonly ActiveWindowTracker _active;
    private readonly ProcessWatcher _proc;
    private readonly UsbWatcher _usb;
    private readonly PolicyEngine _policy;

    public Worker(ILogger<Worker> logger, ConfigManager config, JsonlLogger jsonl, HookManager hooks, ActiveWindowTracker active, ProcessWatcher proc, UsbWatcher usb, PolicyEngine policy)
    {
        _logger = logger;
        _config = config;
        _jsonl = jsonl;
        _hooks = hooks;
        _active = active;
        _proc = proc;
        _usb = usb;
        _policy = policy;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wire events to logger
        _hooks.OnKey += e => _jsonl.Log(new { type = "key", ts = DateTime.UtcNow, e.Key, e.IsDown });
        _hooks.OnMouse += e => _jsonl.Log(new { type = "mouse", ts = DateTime.UtcNow, e.Button, e.X, e.Y, e.Action });
        _active.OnActiveWindow += e => _jsonl.Log(new { type = "activeWindow", ts = DateTime.UtcNow, e.ProcessName, e.WindowTitle });
        _proc.OnProcess += e => _jsonl.Log(new { type = "process", ts = DateTime.UtcNow, e.ProcessName, e.Pid, e.Action });
        _usb.OnUsb += e => _jsonl.Log(new { type = "usb", ts = DateTime.UtcNow, e.DriveLetter, e.Action });

        // Start per config
        var cfg = _config.Current;
        _hooks.Start(cfg.Monitoring.EnableKeyboardHook, cfg.Monitoring.EnableMouseHook);
        if (cfg.Monitoring.EnableActiveWindow) _active.Start();
        if (cfg.Monitoring.EnableProcessWatcher) _proc.Start();
        if (cfg.Monitoring.EnableUsbWatcher) _usb.Start();

        _config.OnConfigChanged += c =>
        {
            _hooks.Stop();
            _hooks.Start(c.Monitoring.EnableKeyboardHook, c.Monitoring.EnableMouseHook);
            if (c.Monitoring.EnableActiveWindow && _active != null) _active.Start(); else _active.Stop();
            if (c.Monitoring.EnableProcessWatcher) _proc.Start(); else _proc.Stop();
            if (c.Monitoring.EnableUsbWatcher) _usb.Start(); else _usb.Stop();
        };

        _logger.LogInformation("ChildGuard service started");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        finally
        {
            _hooks.Stop();
            _active.Stop();
            _proc.Stop();
            _usb.Stop();
            await _jsonl.DisposeAsync();
        }
    }
}
