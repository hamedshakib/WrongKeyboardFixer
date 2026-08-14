using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Models;

public class AppSettings
{
    public bool RunOnStartup { get; set; } = false;
    public string Language { get; set; } = Localization.Languages.English;
    public int HotkeyModifier { get; set; } = (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt);
    public Keys HotkeyKey { get; set; } = Keys.Add;

    // تنظیمات نگاشت فارسی به انگلیسی
    public Dictionary<char, char> PersianToEnglishMap { get; set; } = MappingDefaults.GetDefaultPersianToEnglishMap();

    // تنظیمات نگاشت انگلیسی به فارسی
    public Dictionary<char, char> EnglishToPersianMap { get; set; } = MappingDefaults.GetDefaultEnglishToPersianMap();

    // کلماتی که باید با «آ» یا «ژ» شروع شوند (برای اصلاح پس از تبدیل)
    public List<string> WordCorrections { get; set; } = MappingDefaults.GetDefaultWordCorrections();
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