using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ChildGuard.Core.Monitoring;

public sealed class ActiveWindowTracker
{
    public event Action<ActiveWindowEvent>? OnActiveWindow;

    private IntPtr _last = IntPtr.Zero;
    private readonly TimeSpan _interval = TimeSpan.FromMilliseconds(500);
    private CancellationTokenSource? _cts;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                var hWnd = GetForegroundWindow();
                if (hWnd != _last && hWnd != IntPtr.Zero)
                {
                    _last = hWnd;
                    GetWindowThreadProcessId(hWnd, out uint pid);
                    string procName = "unknown";
                    try { procName = Process.GetProcessById((int)pid).ProcessName; } catch { }
                    var title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    OnActiveWindow?.Invoke(new ActiveWindowEvent(procName, title.ToString()));
                }
                await Task.Delay(_interval, _cts.Token);
            }
        }, _cts.Token);
    }

    public void Stop() { _cts?.Cancel(); }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
}

