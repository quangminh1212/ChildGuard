using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using ChildGuard.Core;
using ChildGuard.Core.Config;

namespace ChildGuard.Tray;

public partial class PolicySettingsWindow : Window
{
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private readonly ConfigManager _cfg;

    public PolicySettingsWindow(ConfigManager cfg)
    {
        InitializeComponent();
        _cfg = cfg;
        LoadData();
        WireEvents();
    }

    private void LoadData()
    {
        var c = _cfg.Current;
        TbQuietStart.Text = c.Policy.QuietHoursStart;
        TbQuietEnd.Text = c.Policy.QuietHoursEnd;
        LbBlocked.ItemsSource = c.Policy.BlockedProcesses.ToList();
        LbAllowedQH.ItemsSource = c.Policy.AllowedProcessesDuringQuietHours.ToList();
    }

    private void WireEvents()
    {
        BtnQuietSave.Click += (_, __) =>
        {
            var c = _cfg.Current;
            c.Policy.QuietHoursStart = TbQuietStart.Text.Trim();
            c.Policy.QuietHoursEnd = TbQuietEnd.Text.Trim();
            _cfg.Save(c);
            System.Windows.MessageBox.Show("Saved Quiet Hours.");
        };
        BtnQuietClose.Click += (_, __) => Close();

        BtnBlockedAdd.Click += (_, __) => { AddToList(TbBlocked.Text, true); };
        BtnBlockedRemove.Click += (_, __) => { RemoveSelected(LbBlocked, true); };
        BtnBlockedAddActive.Click += (_, __) => { AddActive(true); };

        BtnAllowedAdd.Click += (_, __) => { AddToList(TbAllowedQH.Text, false); };
        BtnAllowedRemove.Click += (_, __) => { RemoveSelected(LbAllowedQH, false); };
        BtnAllowedAddActive.Click += (_, __) => { AddActive(false); };
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
}

