using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

public partial class HotkeyManager : IDisposable
{
    private const int WM_HOTKEY = Constants.WindowsMessages.WM_HOTKEY;
    private readonly int _hotkeyId;
    private readonly IntPtr _windowHandle;
    private bool _isRegistered;

    public HotkeyManager(IntPtr windowHandle, int hotkeyId = 1)
    {
        _windowHandle = windowHandle;
        _hotkeyId = hotkeyId;
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    public bool Register(uint modifiers, Keys key)
    {
        if (_isRegistered)
            return true;

        _isRegistered = RegisterHotKey(_windowHandle, _hotkeyId, modifiers, (uint)key);
        return _isRegistered;
    }

    public void Unregister()
    {
        if (!_isRegistered)
            return;

        UnregisterHotKey(_windowHandle, _hotkeyId);
        _isRegistered = false;
    }

    public bool HandleHotkeyMessage(ref Message message)
    {
        return message.Msg == WM_HOTKEY && message.WParam.ToInt32() == _hotkeyId;
    }
}