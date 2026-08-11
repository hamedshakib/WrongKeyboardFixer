using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Application.CommonAppDataPath,
        "WrongKeyboardFixer",
        "settings.json"
    );

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
            var result = settings ?? new AppSettings();

            // Initialize custom mappings from defaults if empty
            if (result.PersianToEnglishMap == null || result.PersianToEnglishMap.Count == 0)
            {
                result.PersianToEnglishMap = InitializeDefaultPersianToEnglishMap();
            }

            if (result.EnglishToPersianMap == null || result.EnglishToPersianMap.Count == 0)
            {
                result.EnglishToPersianMap = InitializeDefaultEnglishToPersianMap();
            }

            return result;
        }
        catch
        {
            // در صورت خطا، تنظیمات پیش‌فرض برگردانده می‌شود
            return new AppSettings();
        }
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
    /// Get a copy of default Persian to English character mappings.
    /// </summary>
    public static Dictionary<char, char> GetDefaultPersianToEnglishMap()
    {
        return new Dictionary<char, char>(InitializeDefaultPersianToEnglishMap());
    }

    /// <summary>
    /// Get a copy of default English to Persian character mappings.
    /// </summary>
    public static Dictionary<char, char> GetDefaultEnglishToPersianMap()
    {
        return new Dictionary<char, char>(InitializeDefaultEnglishToPersianMap());
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

    public static void Save(AppSettings settings)
    {
        try
        {
            // ایجاد پوشه اگر وجود نداشته باشد
            string directory = Path.GetDirectoryName(SettingsPath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Localization.Format("SaveSettingsError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    public static void AddToStartup(bool enable)
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                true
            );

            if (key == null)
                return;

            if (enable)
            {
                string appPath = Application.ExecutablePath;
                key.SetValue("WrongKeyboardFixer", $"\"{appPath}\"");
            }
            else
            {
                if (key.GetValue("WrongKeyboardFixer") != null)
                    key.DeleteValue("WrongKeyboardFixer");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Localization.Format("StartupSettingsError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}