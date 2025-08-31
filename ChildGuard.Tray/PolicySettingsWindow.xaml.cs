using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using ChildGuard.Core;
using ChildGuard.Core.Config;
using System.Windows.Threading;


namespace ChildGuard.Tray;

public partial class PolicySettingsWindow : Window
{
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();


    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private readonly ConfigManager _cfg;
    private readonly DispatcherTimer _autoTimer = new DispatcherTimer();

    public PolicySettingsWindow(ConfigManager cfg)
    {
        InitializeComponent();
        _cfg = cfg;
        LoadData();
        WireEvents();
        InitAutoRefresh();
    }

    private static bool IsValidTime(string s)
    {
        return TimeSpan.TryParse(s, out _);
    }

    private void LoadData()
    {
        var c = _cfg.Current;
        TbQuietStart.Text = c.Policy.QuietHoursStart;
        TbQuietEnd.Text = c.Policy.QuietHoursEnd;
        TbCountdown.Text = c.Policy.EnforcementCountdownSeconds.ToString();
        TbWarnCooldown.Text = c.Policy.WarningCooldownSeconds.ToString();
        LbBlocked.ItemsSource = c.Policy.BlockedProcesses.ToList();
        LbAllowedQH.ItemsSource = c.Policy.AllowedProcessesDuringQuietHours.ToList();
        ReloadRunning();
    }

    private void InitAutoRefresh()
    {
        _autoTimer.Interval = TimeSpan.FromSeconds(10);
        _autoTimer.Tick += (_, __) => { if (CbAutoRefresh.IsChecked == true) ReloadRunning(); };
        _autoTimer.Start();
        CbAutoInterval.SelectedIndex = 1;
        CbAutoInterval.SelectionChanged += (_, __) =>
        {
            if (CbAutoInterval.SelectedItem is System.Windows.Controls.ComboBoxItem ci && int.TryParse(ci.Content.ToString(), out var sec) && sec > 0)
            {
                _autoTimer.Interval = TimeSpan.FromSeconds(sec);
            }
        };
    }

    private void WireEvents()
    {
        BtnQuietSave.Click += (_, __) =>
        {
            var start = TbQuietStart.Text.Trim();
            var end = TbQuietEnd.Text.Trim();
            if (!IsValidTime(start) || !IsValidTime(end))
            {
                System.Windows.MessageBox.Show("Invalid time format. Use HH:mm (e.g., 21:30).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var c = _cfg.Current;
            c.Policy.QuietHoursStart = start;
            c.Policy.QuietHoursEnd = end;
            _cfg.Save(c);
            System.Windows.MessageBox.Show("Saved Quiet Hours.");
        };

        BtnEnfSave.Click += (_, __) =>
        {
            if (!int.TryParse(TbCountdown.Text.Trim(), out var cd) || cd < 0)
            {
                System.Windows.MessageBox.Show("Countdown must be a non-negative integer (seconds).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(TbWarnCooldown.Text.Trim(), out var wc) || wc < 0)
            {
                System.Windows.MessageBox.Show("Warning Cooldown must be a non-negative integer (seconds).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var c = _cfg.Current;
            c.Policy.EnforcementCountdownSeconds = cd;
            c.Policy.WarningCooldownSeconds = wc;
            _cfg.Save(c);
            System.Windows.MessageBox.Show("Saved Enforcement settings.");
        };

        BtnRunRefresh.Click += (_, __) => ReloadRunning();
        BtnRunAddBlocked.Click += (_, __) => { AddSelectedRunning(true); };

        BtnRunAddAllowed.Click += (_, __) => { AddSelectedRunning(false); };
        BtnQuietClose.Click += (_, __) => Close();

        BtnBlockedAdd.Click += (_, __) => { AddToList(TbBlocked.Text, true); };
        BtnBlockedRemove.Click += (_, __) => { if (LbBlocked.SelectedItem is string n && Confirm($"Remove '{n}' from Blocked?")) RemoveSelected(LbBlocked, true); };
        BtnBlockedAddActive.Click += (_, __) => { AddActive(true); };
        BtnBlockedImport.Click += (_, __) => ImportList(true);
        BtnBlockedExport.Click += (_, __) => ExportList(true);

        BtnAllowedAdd.Click += (_, __) => { AddToList(TbAllowedQH.Text, false); };
        BtnAllowedRemove.Click += (_, __) => { if (LbAllowedQH.SelectedItem is string n && Confirm($"Remove '{n}' from Allowed (Quiet Hours)?")) RemoveSelected(LbAllowedQH, false); };
        BtnAllowedAddActive.Click += (_, __) => { AddActive(false); };
        BtnAllowedImport.Click += (_, __) => ImportList(false);
        BtnAllowedExport.Click += (_, __) => ExportList(false);
    }

    private void AddToList(string text, bool blocked)
    {
        var name = text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var c = _cfg.Current;
        var list = blocked ? c.Policy.BlockedProcesses : c.Policy.AllowedProcessesDuringQuietHours;
        if (!list.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            list.Add(name);
            _cfg.Save(c);
            ReloadLists();
        }
    }

    private void RemoveSelected(System.Windows.Controls.ListBox lb, bool blocked)
    {
        if (lb.SelectedItem is string name)
        {
            var c = _cfg.Current;
            var list = blocked ? c.Policy.BlockedProcesses : c.Policy.AllowedProcessesDuringQuietHours;
            var idx = list.FindIndex(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                list.RemoveAt(idx);
                _cfg.Save(c);
                ReloadLists();
            }
    private bool Confirm(string msg)
    {
        var r = System.Windows.MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        return r == MessageBoxResult.Yes;
    }

        }
    }

    private void AddActive(bool blocked)
    {
        try
        {
            var hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out uint pid);
            var procName = System.Diagnostics.Process.GetProcessById((int)pid).ProcessName;
            AddToList(procName, blocked);
        }
        catch { }
    }

    private void ReloadLists()
    {
        var c = _cfg.Current;
        LbBlocked.ItemsSource = null;
        LbBlocked.ItemsSource = c.Policy.BlockedProcesses.ToList();
        LbAllowedQH.ItemsSource = null;
        LbAllowedQH.ItemsSource = c.Policy.AllowedProcessesDuringQuietHours.ToList();
    }

    private void ReloadRunning()
    {
        try
        {
            var procs = System.Diagnostics.Process.GetProcesses()
                .Select(p => p.ProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList();
            LbRunning.ItemsSource = null;
            LbRunning.ItemsSource = procs;
        }
        catch { }
    }

    private void ImportList(bool blocked)
    {
        try
        {
            var ofd = new Microsoft.Win32.OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (ofd.ShowDialog() == true)
            {
                var lines = System.IO.File.ReadAllLines(ofd.FileName)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var c = _cfg.Current;
                var list = blocked ? c.Policy.BlockedProcesses : c.Policy.AllowedProcessesDuringQuietHours;
                foreach (var name in lines)
                {
                    if (!list.Contains(name, StringComparer.OrdinalIgnoreCase)) list.Add(name);
                }
                _cfg.Save(c);
                ReloadLists();
                System.Windows.MessageBox.Show("Imported.");
            }
        }
        catch { }
    }

    private void ExportList(bool blocked)
    {
        try
        {
            var sfd = new Microsoft.Win32.SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (sfd.ShowDialog() == true)
            {
                var c = _cfg.Current;
                var list = blocked ? c.Policy.BlockedProcesses : c.Policy.AllowedProcessesDuringQuietHours;
                System.IO.File.WriteAllLines(sfd.FileName, list);
                System.Windows.MessageBox.Show("Exported.");
            }
        }
        catch { }
    }

    private void AddSelectedRunning(bool toBlocked)
    {
        if (LbRunning.SelectedItem is string name)
        {
            AddToList(name, toBlocked);
        }
    }

    // Optional: simple auto-refresh toggle every 10s could be added later
}

