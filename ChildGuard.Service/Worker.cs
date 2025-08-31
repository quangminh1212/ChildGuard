using ChildGuard.Core.Config;
using ChildGuard.Core;

using ChildGuard.Core.Logging;
using ChildGuard.Core.Monitoring;
using ChildGuard.Core.Policy;
using ChildGuard.Core.Protection;
using ChildGuard.Core.IPC;

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
    private readonly EnhancedHookAnalyzer _analyzer;
    private readonly EnforcementManager _enforcement;

    private readonly UrlSafetyChecker _urlSafety;
    private AudioMonitor? _audio;
    private readonly Dictionary<int, CancellationTokenSource> _enforcementCts = new();

    public Worker(ILogger<Worker> logger, ConfigManager config, JsonlLogger jsonl, HookManager hooks, ActiveWindowTracker active, ProcessWatcher proc, UsbWatcher usb, PolicyEngine policy, EnhancedHookAnalyzer analyzer, UrlSafetyChecker urlSafety, EnforcementManager enforcement)
    {
        _logger = logger;
        _config = config;
        _jsonl = jsonl;
        _hooks = hooks;
        _active = active;
        _proc = proc;
        _usb = usb;
        _policy = policy;
        _analyzer = analyzer;
        _urlSafety = urlSafety;
        _enforcement = enforcement;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wire events to logger
        _hooks.OnKey += e =>
        {
            _jsonl.Log(new { type = "key", ts = DateTime.UtcNow, e.Key, e.IsDown });
            _analyzer.OnKey(e.Key, e.IsDown);
        };
        _hooks.OnMouse += e => _jsonl.Log(new { type = "mouse", ts = DateTime.UtcNow, e.Button, e.X, e.Y, e.Action });
        _active.OnActiveWindow += e => _jsonl.Log(new { type = "activeWindow", ts = DateTime.UtcNow, e.ProcessName, e.WindowTitle });
        _proc.OnProcess += e =>
        {
            _jsonl.Log(new { type = "process", ts = DateTime.UtcNow, e.ProcessName, e.Pid, e.Action });
            if (e.Action == "start")
            {
                TryEnforceProcess(e.ProcessName, e.Pid);
            }
        };
        _usb.OnUsb += e => _jsonl.Log(new { type = "usb", ts = DateTime.UtcNow, e.DriveLetter, e.Action });

        _analyzer.OnBadWord += word =>
        {
            if (_policy.CanWarn())
            {
                _jsonl.Log(new { type = "alert", level = "warning", ts = DateTime.UtcNow, message = $"Bad word detected: {word}" });
                _policy.MarkWarned();
            }
        // Emit IPC to Tray for URL alerts
        _analyzer.OnUrlDetected += url =>
        {
            if (_urlSafety.IsUnsafe(url, out var rule))
            {
                _jsonl.Log(new { type = "url_alert", level = "warning", ts = DateTime.UtcNow, url, rule });
                try { FileIpc.SendToTray(new IpcMessage("toast", new ToastAlert("Unsafe URL", url, _config.Current.Policy.EnforcementCountdownSeconds, Url: url, Rule: rule))); } catch { }
            }
        };

        };
        _analyzer.OnUrlDetected += url =>
        {
            if (_urlSafety.IsUnsafe(url, out var rule))
            {
                _jsonl.Log(new { type = "url_alert", level = "warning", ts = DateTime.UtcNow, url, rule });
                // IPC to Tray already handled above
            }
        };

        // Start per config
        var cfg = _config.Current;
        _hooks.Start(cfg.Monitoring.EnableKeyboardHook, cfg.Monitoring.EnableMouseHook);
        if (cfg.Monitoring.EnableActiveWindow) _active.Start();
        if (cfg.Monitoring.EnableProcessWatcher) _proc.Start();
        if (cfg.Monitoring.EnableUsbWatcher) _usb.Start();
        if (cfg.Protection.EnableAudioMonitor && !string.IsNullOrWhiteSpace(cfg.Protection.FfmpegPath))
        {
            _audio = new AudioMonitor(cfg.Protection.FfmpegPath!);
            _audio.OnLevel += lvl => _jsonl.Log(new { type = "audio", ts = DateTime.UtcNow, level = lvl });
            _audio.Start();
        }

        _config.OnConfigChanged += c =>
        {
            _hooks.Stop();
            _hooks.Start(c.Monitoring.EnableKeyboardHook, c.Monitoring.EnableMouseHook);
            if (c.Monitoring.EnableActiveWindow) _active.Start(); else _active.Stop();
            if (c.Monitoring.EnableProcessWatcher) _proc.Start(); else _proc.Stop();
            if (c.Monitoring.EnableUsbWatcher) _usb.Start(); else _usb.Stop();
            if (c.Protection.EnableAudioMonitor && !string.IsNullOrWhiteSpace(c.Protection.FfmpegPath))
            {
                _audio?.Stop();
                _audio = new AudioMonitor(c.Protection.FfmpegPath!);
                _audio.OnLevel += lvl => _jsonl.Log(new { type = "audio", ts = DateTime.UtcNow, level = lvl });
                _audio.Start();
            }
            else
            {
                _audio?.Stop();
                _audio = null;
            }
        };

        _logger.LogInformation("ChildGuard service started");

        // cancel pending enforcement when process stops
        _proc.OnProcess += e =>
        {
            if (e.Action == "stop" && _enforcementCts.TryGetValue(e.Pid, out var cts))
            {
                cts.Cancel();
            }
        };

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
        // IPC: receive snooze requests
        Task.Run(async () =>
        {
            Directory.CreateDirectory(Paths.ControlDir);
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var msg in FileIpc.Receive(Paths.ControlDir))
                {
                    if (msg.Type == "snooze")
                    {
                        // simple format parse
                        var json = System.Text.Json.JsonSerializer.Serialize(msg.Payload);
                        var req = System.Text.Json.JsonSerializer.Deserialize<ChildGuard.Core.IPC.EnforcementSnoozeRequest>(json);
                        if (req != null)
                        {
                            _enforcement.Snooze(req.ProcessName, req.Minutes);
                            _jsonl.Log(new { type = "snooze", ts = DateTime.UtcNow, req.ProcessName, req.Minutes });
                        }
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }, stoppingToken);

    }

    private void TryEnforceProcess(string processName, int pid)
    {
        if (_policy.ShouldBlockProcess(processName))
        {
            var cfg = _config.Current.Policy;
            if (cfg.SoftEnforce)
            {
                if (!_policy.CanWarn()) return;
                _jsonl.Log(new { type = "enforce_warn", ts = DateTime.UtcNow, processName, pid, countdown = cfg.EnforcementCountdownSeconds });
                try { FileIpc.SendToTray(new IpcMessage("toast", new ToastAlert("Enforcement countdown", $"{processName} will be closed", cfg.EnforcementCountdownSeconds, Process: processName, DeadlineUtc: DateTime.UtcNow.AddSeconds(cfg.EnforcementCountdownSeconds)))); } catch { }
                _policy.MarkWarned();
                var cts = new CancellationTokenSource();
                _enforcementCts[pid] = cts;
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(cfg.EnforcementCountdownSeconds), cts.Token);
                        SafeKillProcess(pid, processName);
                    }
                    catch (TaskCanceledException) { }
                    finally { _enforcementCts.Remove(pid); }
                });
            }
            else
            {
                SafeKillProcess(pid, processName);
            }
        }
    }

    private void SafeKillProcess(int pid, string processName)
    {
        try
        {
            var p = System.Diagnostics.Process.GetProcessById(pid);
            if (!p.HasExited)
            {
                // Try graceful close first
                if (p.CloseMainWindow())
                {
                    if (p.WaitForExit(5000))
                    {
                        _jsonl.Log(new { type = "enforce_close", ts = DateTime.UtcNow, processName, pid });
                        return;
                    }
                }
                // Fallback to kill
                p.Kill();
                _jsonl.Log(new { type = "enforce_kill", ts = DateTime.UtcNow, processName, pid });
            }
        }
        catch (Exception ex)
        {
            _jsonl.Log(new { type = "enforce_error", ts = DateTime.UtcNow, processName, pid, error = ex.Message });
        }
    }
}

