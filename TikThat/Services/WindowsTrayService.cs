#if WINDOWS
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TikThat.Services;

/// <summary>
/// Lightweight Win32 Notification Area (System Tray) controller.
/// Handles taskbar icon registration, user interactions, context menus, and process teardown.
/// </summary>
public static class WindowsTrayService
{
    private const int WmUser = 0x0400;
    private const int WmTrayIcon = WmUser + 101;

    private const int NimAdd = 0x00000000;
    private const int NimModify = 0x00000001;
    private const int NimDelete = 0x00000002;

    private const int NifMessage = 0x00000001;
    private const int NifIcon = 0x00000002;
    private const int NifTip = 0x00000004;

    private const int WmLbuttonup = 0x0202;
    private const int WmRbuttonup = 0x0205;
    private const int WmLbuttondblclk = 0x0203;

    private const int MfString = 0x00000000;
    private const int MfSeparator = 0x00000800;
    private const int TpmRightalign = 0x0008;
    private const int TpmBottomalign = 0x0020;
    private const int TpmReturncmd = 0x0100;

    private const int CmdShow = 1001;
    private const int CmdExit = 1002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, int uFlags, IntPtr uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lpTPMPARAMS);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASS
    {
        public uint style;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

    private static IntPtr _messageHwnd = IntPtr.Zero;
    private static WndProcDelegate? _wndProcDelegate;
    private static NOTIFYICONDATA _nid;
    private static bool _isInitialized;

    public static void InitializeTray()
    {
        if (_isInitialized) return;

        try
        {
            _wndProcDelegate = TrayWindowProc;

            var wndClass = new WNDCLASS
            {
                lpszClassName = "TikThatTrayListenerClass",
                lpfnWndProc = _wndProcDelegate
            };

            RegisterClass(ref wndClass);

            _messageHwnd = CreateWindowEx(
                0, "TikThatTrayListenerClass", "TikThatTrayListener",
                0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            IntPtr hIcon = LoadIcon(IntPtr.Zero, (IntPtr)32512); // Standard App Icon fallback

            _nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _messageHwnd,
                uID = 1001,
                uFlags = NifMessage | NifIcon | NifTip,
                uCallbackMessage = WmTrayIcon,
                hIcon = hIcon,
                szTip = "تیک‌ دت | مدیریت هوشمند کارها"
            };

            Shell_NotifyIcon(NimAdd, ref _nid);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Tray Init Error]: {ex.Message}");
        }
    }

    public static void RemoveTray()
    {
        if (!_isInitialized) return;

        try
        {
            Shell_NotifyIcon(NimDelete, ref _nid);
            if (_messageHwnd != IntPtr.Zero)
            {
                DestroyWindow(_messageHwnd);
                _messageHwnd = IntPtr.Zero;
            }
            _isInitialized = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Tray Removal Error]: {ex.Message}");
        }
    }

    private static IntPtr TrayWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WmTrayIcon)
        {
            int eventId = lParam.ToInt32();

            // دابل کلیک یا کلیک چپ: باز کردن پنجره
            if (eventId is WmLbuttonup or WmLbuttondblclk)
            {
                RestoreMainWindow();
            }
            // کلیک راست: باز کردن منوی خروج
            else if (eventId == WmRbuttonup)
            {
                ShowContextMenu();
            }
            return IntPtr.Zero;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static void ShowContextMenu()
    {
        IntPtr hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MfString, (IntPtr)CmdShow, "نمایش تیک‌ دت");
        AppendMenu(hMenu, MfSeparator, IntPtr.Zero, string.Empty);
        AppendMenu(hMenu, MfString, (IntPtr)CmdExit, "خروج کامل");

        GetCursorPos(out var pt);
        SetForegroundWindow(_messageHwnd);

        int cmd = TrackPopupMenuEx(hMenu, TpmRightalign | TpmBottomalign | TpmReturncmd, pt.X, pt.Y, _messageHwnd, IntPtr.Zero);
        DestroyMenu(hMenu);

        if (cmd == CmdShow)
        {
            RestoreMainWindow();
        }
        else if (cmd == CmdExit)
        {
            RemoveTray();
            Environment.Exit(0);
        }
    }

    private static void RestoreMainWindow()
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            if (App.MainWindow != null)
            {
                App.MainWindow.Show();
            }
        });
    }
}
#endif