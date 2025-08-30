using System.Runtime.InteropServices;
using ChildGuard.Core.Configuration;
using ChildGuard.Core.Models;
using ChildGuard.Core.Diagnostics;

namespace ChildGuard.Hooking;

public sealed class HookManager : IDisposable
{
    private Thread? _hookThread;
    private IntPtr _kbHook = IntPtr.Zero;
    private IntPtr _mouseHook = IntPtr.Zero;
    private volatile bool _running;

    private long _keyPressCount;
    private long _mouseEventCount;

    public event Action<ChildGuard.Core.Models.ActivityEvent>? OnEvent;

    public bool Start(AppConfig config)
    {
        if (_running) return true;
        _running = true;
        SimpleLogger.Info("HookManager.Start called; EnableInputMonitoring={0}", config.EnableInputMonitoring);

        _hookThread = new Thread(() => HookThreadProc(config))
        {
            IsBackground = true,
            Name = "ChildGuard.HookThread"
        };
        _hookThread.SetApartmentState(ApartmentState.STA);
        _hookThread.Start();
        return true;
    }

    public void Stop()
    {
        _running = false;
        SimpleLogger.Info("HookManager.Stop called");
        if (_kbHook != IntPtr.Zero)
        {
            Native.UnhookWindowsHookEx(_kbHook);
            _kbHook = IntPtr.Zero;
        }
        if (_mouseHook != IntPtr.Zero)
        {
            Native.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
        // Post a WM_QUIT to end message loop
        Native.PostThreadMessage(Native.GetCurrentThreadIdSafe(_hookThread), 0x0012 /*WM_QUIT*/, IntPtr.Zero, IntPtr.Zero);
    }

    private void HookThreadProc(AppConfig config)
    {
        IntPtr hInstance = Native.GetModuleHandle(null);

        if (config.EnableInputMonitoring)
        {
            _kbHook = Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, KeyboardProc, hInstance, 0);
            _mouseHook = Native.SetWindowsHookEx(Native.WH_MOUSE_LL, MouseProc, hInstance, 0);
            SimpleLogger.Info("Low-level KB/Mouse hooks installed");
        }
        else
        {
            SimpleLogger.Info("Input monitoring disabled; hooks not installed");
        }

        // Simple message loop to keep hooks alive
        Native.MSG msg;
        while (_running && Native.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
        {
            Native.TranslateMessage(ref msg);
            Native.DispatchMessage(ref msg);
        }

        // Ensure unhook on exit
        if (_kbHook != IntPtr.Zero)
        {
            Native.UnhookWindowsHookEx(_kbHook);
            _kbHook = IntPtr.Zero;
        }
        if (_mouseHook != IntPtr.Zero)
        {
            Native.UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
        }
    }

    private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            // For privacy, do NOT record the specific key. Only count events.
            Interlocked.Increment(ref _keyPressCount);
            OnEvent?.Invoke(new ChildGuard.Core.Models.ActivityEvent(DateTimeOffset.Now, ActivityEventType.Keyboard, new InputActivitySummary(_keyPressCount, _mouseEventCount)));
        }
        return Native.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            Interlocked.Increment(ref _mouseEventCount);
            OnEvent?.Invoke(new ChildGuard.Core.Models.ActivityEvent(DateTimeOffset.Now, ActivityEventType.Mouse, new InputActivitySummary(_keyPressCount, _mouseEventCount)));
        }
        return Native.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    public void Dispose() => Stop();

    private static class Native
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public int pt_x;
            public int pt_y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostThreadMessage(uint idThread, uint Msg, IntPtr wParam, IntPtr lParam);

        public static uint GetCurrentThreadIdSafe(Thread? thread)
        {
            // This is a simplification; PostThreadMessage requires the target thread have a message queue
            // Use stable managed thread id instead of deprecated AppDomain.GetCurrentThreadId
            return thread is null ? 0 : (uint)thread.ManagedThreadId;
        }

        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
    }
}
