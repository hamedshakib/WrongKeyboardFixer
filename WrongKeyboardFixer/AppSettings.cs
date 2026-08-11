using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public class AppSettings
{
    public bool RunOnStartup { get; set; } = false;
    public string Language { get; set; } = Localization.Languages.English;
    public int HotkeyModifier { get; set; } = (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt);
    public Keys HotkeyKey { get; set; } = Keys.Add;

    // تنظیمات پیشرفته
    public int ClipboardRetryDelay { get; set; } = 150;
    public int ClipboardMaxRetries { get; set; } = 8;
    public int KeySimulationDelay { get; set; } = 30;

    // تنظیمات نگاشت فارسی به انگلیسی
    public Dictionary<char, char> PersianToEnglishMap { get; set; } = MappingDefaults.GetDefaultPersianToEnglishMap();

    // تنظیمات نگاشت انگلیسی به فارسی
    public Dictionary<char, char> EnglishToPersianMap { get; set; } = MappingDefaults.GetDefaultEnglishToPersianMap();
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