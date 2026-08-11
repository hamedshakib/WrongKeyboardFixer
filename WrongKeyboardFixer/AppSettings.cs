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
    public Dictionary<char, char> PersianToEnglishMap { get; set; }

    // تنظیمات نگاشت انگلیسی به فارسی
    public Dictionary<char, char> EnglishToPersianMap { get; set; }

    // پیش‌فرض سازنده - مقداردهی دیکشنری‌ها با مپ‌های پیش‌فرض
    public AppSettings()
    {
        PersianToEnglishMap = InitializeDefaultPersianToEnglishMap();
        EnglishToPersianMap = InitializeDefaultEnglishToPersianMap();
    }

    /// <summary>
    /// Initialize default Persian to English character mappings.
    /// </summary>
    private static Dictionary<char, char> InitializeDefaultPersianToEnglishMap()
    {
        var mappings = new Dictionary<char, char>();

        // Standard Persian to English character mapping
        mappings['ض'] = 'q'; mappings['ص'] = 'w'; mappings['ث'] = 'e'; mappings['ق'] = 'r'; mappings['ف'] = 't'; mappings['غ'] = 'y'; mappings['ع'] = 'u'; mappings['ه'] = 'i'; mappings['خ'] = 'o'; mappings['ح'] = 'p'; mappings['ج'] = '['; mappings['چ'] = ']'; mappings['پ'] = '\\';
        mappings['ش'] = 'a'; mappings['س'] = 's'; mappings['ی'] = 'd'; mappings['ب'] = 'f'; mappings['ل'] = 'g'; mappings['ا'] = 'h'; mappings['ت'] = 'j'; mappings['ن'] = 'k'; mappings['م'] = 'l'; mappings['ک'] = ';'; mappings['گ'] = '\'';
        mappings['ظ'] = 'z'; mappings['ط'] = 'x'; mappings['ز'] = 'c'; mappings['ر'] = 'v'; mappings['ذ'] = 'b'; mappings['د'] = 'n'; mappings['ئ'] = 'm'; mappings['و'] = ','; mappings['.'] = '.'; mappings['/'] = '/';
        mappings['۰'] = '0'; mappings['۱'] = '1'; mappings['۲'] = '2'; mappings['۳'] = '3'; mappings['۴'] = '4'; mappings['۵'] = '5'; mappings['۶'] = '6'; mappings['۷'] = '7'; mappings['۸'] = '8'; mappings['۹'] = '9';
        mappings['÷'] = '`';
        return mappings;
    }

    /// <summary>
    /// Initialize default English to Persian character mappings.
    /// </summary>
    private static Dictionary<char, char> InitializeDefaultEnglishToPersianMap()
    {
        var mappings = new Dictionary<char, char>();

        // Standard English to Persian character mapping
        mappings['q'] = 'ض'; mappings['w'] = 'ص'; mappings['e'] = 'ث'; mappings['r'] = 'ق'; mappings['t'] = 'ف'; mappings['y'] = 'غ'; mappings['u'] = 'ع'; mappings['i'] = 'ه'; mappings['o'] = 'خ'; mappings['p'] = 'ح'; mappings['['] = 'ج'; mappings[']'] = 'چ'; mappings['\\'] = 'پ';
        mappings['a'] = 'ش'; mappings['s'] = 'س'; mappings['d'] = 'ی'; mappings['f'] = 'ب'; mappings['g'] = 'ل'; mappings['h'] = 'ا'; mappings['j'] = 'ت'; mappings['k'] = 'ن'; mappings['l'] = 'م'; mappings[';'] = 'ک'; mappings['\''] = 'گ';
        mappings['z'] = 'ظ'; mappings['x'] = 'ط'; mappings['c'] = 'ز'; mappings['v'] = 'ر'; mappings['b'] = 'ذ'; mappings['n'] = 'د'; mappings['m'] = 'ئ'; mappings[','] = 'و'; mappings['.'] = '.'; mappings['/'] = '/';
        mappings['0'] = '۰'; mappings['1'] = '۱'; mappings['2'] = '۲'; mappings['3'] = '۳'; mappings['4'] = '۴'; mappings['5'] = '۵'; mappings['6'] = '۶'; mappings['7'] = '۷'; mappings['8'] = '۸'; mappings['9'] = '۹';

        mappings['`'] = '÷';
        return mappings;
    }
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