using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using ChildGuard.Core;
using ChildGuard.Core.Config;
using ChildGuard.Core.IPC;

namespace ChildGuard.Tray;

public sealed class TrayApp : IDisposable
{
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    private readonly NotifyIcon _notify;
    private readonly ConfigManager _cfg;
    private readonly FileSystemWatcher _logWatcher;
    private readonly string _logDir = Paths.LogsDir;

    public TrayApp()
    {
        _cfg = new ConfigManager(Paths.ConfigFile);

        _notify = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Shield,
            Visible = true,
            Text = "ChildGuard"
        };
        _notify.ContextMenuStrip = BuildMenu();

        Directory.CreateDirectory(_logDir);
        _logWatcher = new FileSystemWatcher(_logDir, "events-*.jsonl")
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _logWatcher.Changed += (_, __) => TryReadLatestForAlerts();
        _logWatcher.EnableRaisingEvents = true;
        // IPC listener: show rich toast from Service
        Task.Run(async () =>
        {
            Directory.CreateDirectory(Paths.ControlTrayInbox);
            while (true)
            {
                foreach (var msg in FileIpc.Receive(Paths.ControlTrayInbox))
                {
                    if (msg.Type == "toast")
                    {
                        try
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(msg.Payload);
                            var toast = System.Text.Json.JsonSerializer.Deserialize<ChildGuard.Core.IPC.ToastAlert>(json);
                            if (toast != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var tw = new ToastWindow();
                                    tw.ShowToast(toast.Message, toast.CountdownSeconds);
                                });
                            }
                        }
                        catch { }
                    }
                }
                await Task.Delay(500);
            }
        });

    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        var mKeyboard = new ToolStripMenuItem("Keyboard Hook") { Checked = _cfg.Current.Monitoring.EnableKeyboardHook, CheckOnClick = true };
        var mMouse = new ToolStripMenuItem("Mouse Hook") { Checked = _cfg.Current.Monitoring.EnableMouseHook, CheckOnClick = true };
        var mActive = new ToolStripMenuItem("Active Window") { Checked = _cfg.Current.Monitoring.EnableActiveWindow, CheckOnClick = true };
        var mSnooze5 = new ToolStripMenuItem("Snooze 5m");
        var mSnooze15 = new ToolStripMenuItem("Snooze 15m");

        var mProc = new ToolStripMenuItem("Process Watcher") { Checked = _cfg.Current.Monitoring.EnableProcessWatcher, CheckOnClick = true };
        var mUsb = new ToolStripMenuItem("USB Watcher") { Checked = _cfg.Current.Monitoring.EnableUsbWatcher, CheckOnClick = true };
        var mQuit = new ToolStripMenuItem("Exit");

        mKeyboard.CheckedChanged += (_, __) => { var c = _cfg.Current; c.Monitoring.EnableKeyboardHook = mKeyboard.Checked; _cfg.Save(c); };
        mMouse.CheckedChanged +=    (_, __) => { var c = _cfg.Current; c.Monitoring.EnableMouseHook = mMouse.Checked; _cfg.Save(c); };
        mActive.CheckedChanged +=   (_, __) => { var c = _cfg.Current; c.Monitoring.EnableActiveWindow = mActive.Checked; _cfg.Save(c); };
        mProc.CheckedChanged +=     (_, __) => { var c = _cfg.Current; c.Monitoring.EnableProcessWatcher = mProc.Checked; _cfg.Save(c); };
        mUsb.CheckedChanged +=      (_, __) => { var c = _cfg.Current; c.Monitoring.EnableUsbWatcher = mUsb.Checked; _cfg.Save(c); };
        mQuit.Click += (_, __) => System.Windows.Application.Current.Shutdown();

        mSnooze5.Click += (_, __) => SendSnooze(5);
        mSnooze15.Click += (_, __) => SendSnooze(15);

        menu.Items.AddRange(new ToolStripItem[] { mKeyboard, mMouse, mActive, new ToolStripSeparator(), mSnooze5, mSnooze15, new ToolStripSeparator(), mProc, mUsb, new ToolStripSeparator(), mQuit });
        return menu;
    }

    private void TryReadLatestForAlerts()
    {
        try
        {
            var latest = Directory.GetFiles(_logDir, "events-*.jsonl")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();
            if (latest == null) return;
            using var fs = new FileStream(latest.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            string? line;
            string? lastAlert = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("\"url_alert\"") || line.Contains("\"alert\""))
                {
                    lastAlert = line;
                }
            }
            if (lastAlert != null)
            {
                _notify.BalloonTipTitle = "ChildGuard Alert";
                _notify.BalloonTipText = lastAlert.Length > 256 ? lastAlert[..256] + "..." : lastAlert;
                _notify.ShowBalloonTip(3000);
            }
        }
        catch { }
    }

    private void SendSnooze(int minutes)
    {
        // For simplicity, snooze applies to active foreground process
        try
        {
            // Get real foreground process
            var hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out uint pid);
            var procName = Process.GetProcessById((int)pid).ProcessName;
            var msg = new IpcMessage("snooze", new EnforcementSnoozeRequest(procName, minutes));
            FileIpc.Send(Paths.ControlDir, msg);
            _notify.BalloonTipTitle = "ChildGuard";
            _notify.BalloonTipText = $"Snoozed enforcement for {procName} in {minutes}m";
            _notify.ShowBalloonTip(2000);
        }
        catch { }
    }

    public void Dispose()
    {
        _notify.Visible = false;
        _notify.Dispose();
        _logWatcher.Dispose();
    }
}

