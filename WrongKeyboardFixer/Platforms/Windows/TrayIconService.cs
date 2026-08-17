using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WrongKeyboardFixer.Platforms.Windows;

/// <summary>
/// Manages the system tray icon using Win32 Shell_NotifyIcon API.
/// Provides context menu with Settings and Exit options.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private const int NOTIFYICON_VERSION_4 = 4;
    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;
    private const int NIM_SETVERSION = 0x00000004;

    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_COMMAND = 0x0111;

    private const int ID_SETTINGS = 1001;
    private const int ID_EXIT = 1002;

    private NOTIFYICONDATA _iconData;
    private IntPtr _hIcon;
    private bool _disposed;
    private IntPtr _ownerWindow;

    public event EventHandler? SettingsRequested;
    public event EventHandler? ExitRequested;

    public void Initialize(Microsoft.UI.Xaml.Window window)
    {
        _ownerWindow = WindowNative.GetWindowHandle(window);

        // Load the app icon
        _hIcon = LoadAppIcon();

        _iconData = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _ownerWindow,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = 0x8000, // Custom callback message
            hIcon = _hIcon,
            szTip = Core.Helpers.Localization.Get("TrayText")
        };

        Shell_NotifyIcon(NIM_ADD, ref _iconData);

        // Set version for callback
        _iconData.uVersion = NOTIFYICON_VERSION_4;
        Shell_NotifyIcon(NIM_SETVERSION, ref _iconData);

        // Hook the window message to handle tray callbacks
        WindowMessageMonitor?.Dispose();
        WindowMessageMonitor = new WindowMessageHook(_ownerWindow);
        WindowMessageMonitor.MessageReceived += OnWindowMessage;
    }

    private WindowMessageHook? WindowMessageMonitor;

    private void OnWindowMessage(uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == _iconData.uCallbackMessage)
        {
            int loWord = lParam.ToInt32() & 0xFFFF;
            switch (loWord)
            {
                case WM_LBUTTONDBLCLK:
                    SettingsRequested?.Invoke(this, EventArgs.Empty);
                    break;
                case WM_RBUTTONUP:
                    ShowContextMenu();
                    break;
            }
        }
    }

    private void ShowContextMenu()
    {
        // Use a WinForms-style popup menu via P/Invoke
        var menu = CreatePopupMenu();
        AppendMenu(menu, 0, ID_SETTINGS, Core.Helpers.Localization.Get("TraySettings"));
        AppendMenu(menu, 0x0800, 0, "-"); // MF_SEPARATOR
        AppendMenu(menu, 0, ID_EXIT, Core.Helpers.Localization.Get("TrayExit"));

        GetCursorPos(out POINT pt);
        SetForegroundWindow(_ownerWindow);

        int cmd = TrackPopupMenu(menu, 0x0100, pt.X, pt.Y, 0, _ownerWindow, IntPtr.Zero);
        if (cmd == ID_SETTINGS)
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        else if (cmd == ID_EXIT)
            ExitRequested?.Invoke(this, EventArgs.Empty);

        DestroyMenu(menu);
    }

    public void UpdateTooltip(string text)
    {
        _iconData.szTip = text;
        Shell_NotifyIcon(NIM_MODIFY, ref _iconData);
    }

    private IntPtr LoadAppIcon()
    {
        try
        {
            string exePath = Environment.ProcessPath ?? "";
            return ExtractIcon(IntPtr.Zero, exePath, 0);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        WindowMessageMonitor?.Dispose();
        Shell_NotifyIcon(NIM_DELETE, ref _iconData);

        if (_hIcon != IntPtr.Zero)
            DestroyIcon(_hIcon);
    }

    #region P/Invoke

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
        public int uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [LibraryImport("shell32.dll", EntryPoint = "Shell_NotifyIconW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT lpPoint);

    [LibraryImport("user32.dll")]
    private static partial IntPtr CreatePopupMenu();

    [LibraryImport("user32.dll", EntryPoint = "AppendMenuW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyMenu(IntPtr hMenu);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [LibraryImport("user32.dll")]
    private static partial void SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("shell32.dll", EntryPoint = "ExtractIconW")]
    private static partial IntPtr ExtractIcon(IntPtr hInst, string file, int index);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyIcon(IntPtr hIcon);

    #endregion
}

/// <summary>
/// Monitors window messages on a background thread by subclassing the window procedure.
/// </summary>
internal sealed class WindowMessageHook : IDisposable
{
    private delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData);

    private readonly IntPtr _hWnd;
    private SUBCLASSPROC? _procDelegate;
    private IntPtr _procPtr;
    private bool _disposed;

    public event Action<uint, IntPtr, IntPtr>? MessageReceived;

    public WindowMessageHook(IntPtr hWnd)
    {
        _hWnd = hWnd;
        _procDelegate = SubclassProc;
        _procPtr = Marshal.GetFunctionPointerForDelegate(_procDelegate);
        SetWindowSubclass(hWnd, _procPtr, 1, IntPtr.Zero);
    }

    [UnmanagedCallersOnly]
    private IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, IntPtr dwRefData)
    {
        MessageReceived?.Invoke(uMsg, wParam, lParam);
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        RemoveWindowSubclass(_hWnd, _procPtr, 1);
    }

    [LibraryImport("comctl32.dll")]
    private static partial IntPtr SetWindowSubclass(IntPtr hWnd, IntPtr pfnSubclass, IntPtr uIdSubclass, IntPtr dwRefData);

    [LibraryImport("comctl32.dll")]
    private static partial IntPtr RemoveWindowSubclass(IntPtr hWnd, IntPtr pfnSubclass, IntPtr uIdSubclass);

    [LibraryImport("comctl32.dll")]
    private static partial IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
}

/// <summary>
/// Win32 WindowNative helper for getting window handle.
/// </summary>
internal static class WindowNative
{
    public static IntPtr GetWindowHandle(Microsoft.UI.Xaml.Window window)
    {
        return WinRT.Interop.WindowNative.GetWindowHandle(window);
    }
}
