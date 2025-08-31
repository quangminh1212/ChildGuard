using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using ChildGuard.Core;
using System.Windows.Media;

using ChildGuard.Core.IPC;

namespace ChildGuard.Tray;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;
    private DateTime _end;
    // Icon color tweak per severity
    private void SetIcon(string? severity)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC29400"); // default (warning-like)
        if (string.Equals(severity, "error", StringComparison.OrdinalIgnoreCase))
            color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFD9534F");
        else if (string.Equals(severity, "info", StringComparison.OrdinalIgnoreCase))
            color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF5BC0DE");
        IconDot.Fill = new SolidColorBrush(color);
    }


    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    public ToastWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, __) => UpdateCountdown();
        Snooze5Btn.Click += (_, __) => { SendSnooze(5); Close(); };
        Snooze15Btn.Click += (_, __) => { SendSnooze(15); Close(); };
        CloseBtn.Click += (_, __) => Close();
    }

    public void ShowToast(string title, string message, int countdownSeconds, DateTime? deadlineUtc = null, string? url = null, string? process = null, string? rule = null, string? severity = null, string? primaryAction = null, string? primaryActionLabel = null)
    {
        TitleText.Text = string.IsNullOrWhiteSpace(title) ? "ChildGuard" : title;
        _end = deadlineUtc ?? DateTime.UtcNow.AddSeconds(countdownSeconds);
        var remain = Math.Max(0, (int)(_end - DateTime.UtcNow).TotalSeconds);
        MessageText.Text = message + $"\nClosing in {remain}s...";
        DetailsText.Text = BuildDetails(url, process, rule);
        ApplySeverity(severity);
        SetIcon(severity);
        ConfigurePrimaryAction(primaryAction, primaryActionLabel);
        ConfigureUrlActions(url);
        _timer.Start();
        PositionBottomRight();
        Show();
        Activate();
    }

    private void ConfigureUrlActions(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        var copyBtn = new System.Windows.Controls.Button { Content = "Copy URL", Padding = new Thickness(8,4,8,4), Margin = new Thickness(8,0,0,0) };
        copyBtn.Click += (_, __) => {
            try { System.Windows.Clipboard.SetText(url); } catch { }
        };
        if (CloseBtn.Parent is System.Windows.Controls.Panel p)
        {
            var idx = Math.Max(0, p.Children.IndexOf(CloseBtn));
            p.Children.Insert(idx, copyBtn);
        }
    }

    private void ApplySeverity(string? severity)
    {
        var brush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#FF2D2D30");
        if (string.Equals(severity, "warning", StringComparison.OrdinalIgnoreCase))
            brush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#FF3B2F14");
        else if (string.Equals(severity, "error", StringComparison.OrdinalIgnoreCase))
            brush = (System.Windows.Media.Brush)new BrushConverter().ConvertFromString("#FF3A1F1F");
        RootBorder.Background = brush;
    }

    private void ConfigurePrimaryAction(string? primaryAction, string? label)
    {
        if (string.IsNullOrWhiteSpace(primaryAction)) return;
        var btn = new System.Windows.Controls.Button { Content = label ?? "Open", Padding = new Thickness(8,4,8,4), Margin = new Thickness(8,0,0,0) };
        btn.Click += (_, __) => HandlePrimaryAction(primaryAction);
        // Insert before CloseBtn
        if (CloseBtn.Parent is System.Windows.Controls.Panel p)
        {
            var idx = Math.Max(0, p.Children.IndexOf(CloseBtn));
            p.Children.Insert(idx, btn);
        }
    }

    private void HandlePrimaryAction(string action)
    {
        try
        {
            if (string.Equals(action, "open_config", StringComparison.OrdinalIgnoreCase))
            {
                // Placeholder: open config file in notepad
                Process.Start(new ProcessStartInfo { FileName = Paths.ConfigFile, UseShellExecute = true });
            }
            else if (string.Equals(action, "open_logs", StringComparison.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo { FileName = Paths.LogsDir, UseShellExecute = true });
            }
        }
        catch { }
    }

    private static string BuildDetails(string? url, string? process, string? rule)
    {
        var lines = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrWhiteSpace(process)) lines.Add($"Process: {process}");
        if (!string.IsNullOrWhiteSpace(url)) lines.Add($"URL: {url}");
        if (!string.IsNullOrWhiteSpace(rule)) lines.Add($"Rule: {rule}");
        return string.Join("\n", lines);
    }

    private void UpdateCountdown()
    {
        var remain = (int)Math.Max(0, (_end - DateTime.UtcNow).TotalSeconds);
        MessageText.Text = MessageText.Text.Split('\n')[0] + $"\nClosing in {remain}s...";
        if (remain <= 0)
        {
            _timer.Stop();
            Close();
        }
    }

    private void PositionBottomRight()
    {
        var desktopWorkingArea = SystemParameters.WorkArea;
        Left = desktopWorkingArea.Right - Width - 20;
        Top = desktopWorkingArea.Bottom - Height - 20;
    }

    private void SendSnooze(int minutes)
    {
        try
        {
            var hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out uint pid);
            var procName = Process.GetProcessById((int)pid).ProcessName;
            var msg = new IpcMessage("snooze", new EnforcementSnoozeRequest(procName, minutes));
            FileIpc.Send(Paths.ControlDir, msg);
        }
        catch { }
    }
}

