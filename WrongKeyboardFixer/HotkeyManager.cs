using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public static class HotkeyModifier
{
    public const uint Alt = 0x0001;
    public const uint Control = 0x0002;
    public const uint Shift = 0x0004;
    public const uint Windows = 0x0008;
    public const uint ControlAlt = Control | Alt;
}

public class HotkeyManager : IDisposable
{
    private readonly IntPtr _windowHandle;
    private readonly int _hotkeyId;
    private bool _isRegistered;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;

    public HotkeyManager(IntPtr windowHandle, int hotkeyId = 1)
    {
        _windowHandle = windowHandle;
        _hotkeyId = hotkeyId;
    }

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

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }
}