using System;
using System.Collections.Generic;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Models;

public class AppSettings
{
    public bool RunOnStartup { get; set; } = false;
    public string Language { get; set; } = Localization.Languages.English;
    public int HotkeyModifier { get; set; } = (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt);
    public int HotkeyKey { get; set; } = 107; // Default: Keys.Add = 107

    public Dictionary<char, char> PersianToEnglishMap { get; set; } = MappingDefaults.GetDefaultPersianToEnglishMap();
    public Dictionary<char, char> EnglishToPersianMap { get; set; } = MappingDefaults.GetDefaultEnglishToPersianMap();
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

/// <summary>
/// Virtual key codes used for hotkey registration (matching Win32 VK codes).
/// </summary>
public static class VirtualKeys
{
    public const int Add = 0x6B;        // Numpad +
    public const int Subtract = 0x6D;   // Numpad -
    public const int Multiply = 0x6A;   // Numpad *
    public const int F1 = 0x70;
    public const int F2 = 0x71;
    public const int F3 = 0x72;
    public const int F4 = 0x73;
    public const int F5 = 0x74;
    public const int F6 = 0x75;
    public const int F7 = 0x76;
    public const int F8 = 0x77;
    public const int F9 = 0x78;
    public const int F10 = 0x79;
    public const int F11 = 0x7A;
    public const int F12 = 0x7B;
    public const int Insert = 0x2D;
    public const int Home = 0x24;
    public const int PageUp = 0x21;
    public const int PageDown = 0x22;
    public const int End = 0x23;
    public const int Delete = 0x2E;
    public const int Space = 0x20;
}
