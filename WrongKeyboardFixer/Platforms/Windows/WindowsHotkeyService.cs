using System.Runtime.InteropServices;

namespace WrongKeyboardFixer.Platforms.Windows;

/// <summary>
/// Windows-only service for registering global hotkeys using Win32 RegisterHotKey API.
/// Creates a hidden message-only window on a background thread to receive WM_HOTKEY.
/// </summary>
public sealed class WindowsHotkeyService : IDisposable
{
    private const int HOTKEY_ID = 1;
    private const string WindowClassName = "WKF_HotkeySink";

    private IntPtr _windowHandle;
    private bool _isRegistered;
    private bool _disposed;
    private WNDPROC? _wndProcDelegate;

    public event EventHandler? HotkeyPressed;

    private void EnsureWindowCreated()
    {
        if (_windowHandle != IntPtr.Zero)
            return;

        _wndProcDelegate = WndProc;

        var thread = new Thread(() =>
        {
            _windowHandle = CreateMessageWindow(WindowClassName, _wndProcDelegate!);
            if (_windowHandle == IntPtr.Zero)
                return;

            // Message loop
            while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        })
        {
            IsBackground = true,
            Name = "HotkeyMessageLoop"
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // Wait for window creation (up to 500ms)
        for (int i = 0; i < 50 && _windowHandle == IntPtr.Zero; i++)
            Thread.Sleep(10);
    }

    public bool Register(uint modifiers, int key)
    {
        if (_isRegistered)
            return true;

        EnsureWindowCreated();
        if (_windowHandle == IntPtr.Zero)
            return false;

        _isRegistered = RegisterHotKey(_windowHandle, HOTKEY_ID, modifiers, key);
        return _isRegistered;
    }

    public void Unregister()
    {
        if (!_isRegistered || _windowHandle == IntPtr.Zero)
            return;

        UnregisterHotKey(_windowHandle, HOTKEY_ID);
        _isRegistered = false;
    }

    [UnmanagedCallersOnly]
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == 0x0312 && wParam.ToInt32() == HOTKEY_ID) // WM_HOTKEY
        {
            ThreadPool.QueueUserWorkItem(_ => HotkeyPressed?.Invoke(this, EventArgs.Empty));
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private static IntPtr CreateMessageWindow(string className, WNDPROC wndProc)
    {
        // RegisterClassEx + CreateWindowEx for a message-only window
        var wc = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
            lpszClassName = className,
        };

        ushort atom = RegisterClassEx(ref wc);
        if (atom == 0)
        {
            // Class may already exist, try GetClassInfo
            if (GetClassInfoEx(IntPtr.Zero, className, out _))
                atom = 1; // Use any non-zero value
        }

        return CreateWindowEx(0, className, "", 0, 0, 0, 0, 0,
            new IntPtr(-3) /* HWND_MESSAGE */, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unregister();
        if (_windowHandle != IntPtr.Zero)
        {
            PostMessage(_windowHandle, 0x0012, IntPtr.Zero, IntPtr.Zero); // WM_QUIT
            _windowHandle = IntPtr.Zero;
        }
    }

    #region P/Invoke

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public uint cbSize;
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
        public IntPtr hIconSm;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int ptX;
        public int ptY;
    }

    [LibraryImport("user32.dll", EntryPoint = "RegisterClassExW")]
    private static partial ushort RegisterClassEx(ref WNDCLASSEX lpWndClass);

    [LibraryImport("user32.dll", EntryPoint = "GetClassInfoExW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetClassInfoEx(IntPtr hInstance, string lpClassName, out WNDCLASSEX lpWndClass);

    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW")]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMin, uint wMax);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TranslateMessage(ref MSG lpMsg);

    [LibraryImport("user32.dll")]
    private static partial IntPtr DispatchMessage(ref MSG lpMsg);

    [LibraryImport("user32.dll")]
    private static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    #endregion
}
