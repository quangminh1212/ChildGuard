using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ChildGuard.Core.Monitoring;

public sealed class HookManager : IDisposable
{
    // Low-level keyboard/mouse hooks
    private IntPtr _kbHook = IntPtr.Zero;
    private IntPtr _msHook = IntPtr.Zero;
    private WinApi.HookProc? _kbProc;
    private WinApi.HookProc? _msProc;

    public event Action<KeyEvent>? OnKey;
    public event Action<MouseEvent>? OnMouse;

    public void Start(bool keyboard, bool mouse)
    {
        if (keyboard && _kbHook == IntPtr.Zero)
        {
            _kbProc = KeyboardProc;
            _kbHook = WinApi.SetWindowsHookEx(WinApi.WH_KEYBOARD_LL, _kbProc, WinApi.GetModuleHandle(null), 0);
        }
        if (mouse && _msHook == IntPtr.Zero)
        {
            _msProc = MouseProc;
            _msHook = WinApi.SetWindowsHookEx(WinApi.WH_MOUSE_LL, _msProc, WinApi.GetModuleHandle(null), 0);
        }
    }

    public void Stop()
    {
        if (_kbHook != IntPtr.Zero) { WinApi.UnhookWindowsHookEx(_kbHook); _kbHook = IntPtr.Zero; }
        if (_msHook != IntPtr.Zero) { WinApi.UnhookWindowsHookEx(_msHook); _msHook = IntPtr.Zero; }
    }

    private IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;
            int vkCode = Marshal.ReadInt32(lParam);
            bool down = msg == WinApi.WM_KEYDOWN || msg == WinApi.WM_SYSKEYDOWN;
            bool up = msg == WinApi.WM_KEYUP || msg == WinApi.WM_SYSKEYUP;
            if (down || up)
            {
                OnKey?.Invoke(new KeyEvent(((ConsoleKey)vkCode).ToString(), down));
            }
        }
        return WinApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private IntPtr MouseProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            var ms = Marshal.PtrToStructure<WinApi.MSLLHOOKSTRUCT>(lParam);
            string action = wParam.ToInt32() switch
            {
                WinApi.WM_LBUTTONDOWN => "down",
                WinApi.WM_LBUTTONUP => "up",
                WinApi.WM_RBUTTONDOWN => "down",
                WinApi.WM_RBUTTONUP => "up",
                WinApi.WM_MOUSEMOVE => "move",
                _ => "other"
            };
            string btn = wParam.ToInt32() switch
            {
                WinApi.WM_LBUTTONDOWN or WinApi.WM_LBUTTONUP => "left",
                WinApi.WM_RBUTTONDOWN or WinApi.WM_RBUTTONUP => "right",
                _ => ""
            };
            OnMouse?.Invoke(new MouseEvent(btn, ms.pt.x, ms.pt.y, action));
        }
        return WinApi.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    public void Dispose() => Stop();
}

internal static class WinApi
{
    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    public const int WH_KEYBOARD_LL = 13;
    public const int WH_MOUSE_LL = 14;

    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_SYSKEYUP = 0x0105;

    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_RBUTTONDOWN = 0x0204;
    public const int WM_RBUTTONUP = 0x0205;

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x; public int y; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string? lpModuleName);
}

