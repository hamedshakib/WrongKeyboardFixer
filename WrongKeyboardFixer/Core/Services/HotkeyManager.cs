using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Manages system-wide hotkey registration and handling.
/// </summary>
public sealed partial class HotkeyManager : IHotkeyService
{
    private readonly IntPtr _windowHandle;
    private readonly int _hotkeyId;
    private bool _isRegistered;
    private bool _disposed;

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;

    /// <summary>
    /// Initializes a new instance of the HotkeyManager.
    /// </summary>
    /// <param name="windowHandle">The window handle to receive hotkey messages.</param>
    /// <param name="hotkeyId">The unique identifier for this hotkey.</param>
    public HotkeyManager(IntPtr windowHandle, int hotkeyId = 1)
    {
        _windowHandle = windowHandle;
        _hotkeyId = hotkeyId;
        _isRegistered = false;
        _disposed = false;
    }

    /// <summary>
    /// Registers a hotkey combination.
    /// </summary>
    /// <param name="modifiers">Modifier keys (Ctrl, Alt, Shift, etc.).</param>
    /// <param name="key">The key to register.</param>
    /// <returns>True if registration succeeded; otherwise, false.</returns>
    public bool Register(uint modifiers, Keys key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_isRegistered)
            return true;

        _isRegistered = RegisterHotKey(_windowHandle, _hotkeyId, modifiers, (uint)key);
        return _isRegistered;
    }

    /// <summary>
    /// Unregisters the currently registered hotkey.
    /// </summary>
    public void Unregister()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_isRegistered)
            return;

        UnregisterHotKey(_windowHandle, _hotkeyId);
        _isRegistered = false;
    }

    /// <summary>
    /// Checks if a message is a hotkey message for this instance.
    /// </summary>
    /// <param name="message">The Windows message to check.</param>
    /// <returns>True if the message is a hotkey message for this instance; otherwise, false.</returns>
    public bool HandleHotkeyMessage(ref Message message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return message.Msg == WM_HOTKEY && message.WParam.ToInt32() == _hotkeyId;
    }

    /// <summary>
    /// Releases all resources used by the HotkeyManager.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        Unregister();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}