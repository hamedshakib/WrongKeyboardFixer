using System;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public class AppSettings
{
    public bool RunOnStartup { get; set; } = false;
    public int HotkeyModifier { get; set; } = (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt);
    public Keys HotkeyKey { get; set; } = Keys.Add;
    public bool StartMinimized { get; set; } = true;
    public bool ShowNotifications { get; set; } = true;

    // تنظیمات پیشرفته
    public int ClipboardRetryDelay { get; set; } = 150;
    public int ClipboardMaxRetries { get; set; } = 8;
    public int KeySimulationDelay { get; set; } = 30;
}

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}