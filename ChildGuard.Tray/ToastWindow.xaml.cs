using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using ChildGuard.Core;
using ChildGuard.Core.IPC;

namespace ChildGuard.Tray;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;
    private DateTime _end;

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

    public void ShowToast(string message, int countdownSeconds)
    {
        TitleText.Text = "ChildGuard";
        _end = DateTime.UtcNow.AddSeconds(countdownSeconds);
        MessageText.Text = message + $"\nClosing in {countdownSeconds}s...";
        _timer.Start();
        PositionBottomRight();
        Show();
        Activate();
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

