using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Application.CommonAppDataPath,
        "WrongKeyboardFixer",
        "settings.json"
    );

    public static WrongKeyboardFixer.Core.Model.AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            return new WrongKeyboardFixer.Core.Model.AppSettings();

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
            var result = settings;
            if (result == null) result = new WrongKeyboardFixer.Core.Model.AppSettings();

            // Initialize custom mappings from defaults if empty
            if (result.PersianToEnglishMap == null || result.PersianToEnglishMap.Count == 0)
            {
                result.PersianToEnglishMap = MappingDefaults.GetDefaultPersianToEnglishMap();
            }

            if (result.EnglishToPersianMap == null || result.EnglishToPersianMap.Count == 0)
            {
                result.EnglishToPersianMap = MappingDefaults.GetDefaultEnglishToPersianMap();
            }

            return result;
        }
        catch
        {
            // در صورت خطا، تنظیمات پیش‌فرض بازنشانی می‌شود
            return new WrongKeyboardFixer.Core.Model.AppSettings();
        }
    }

    public static void Save(WrongKeyboardFixer.Core.Model.AppSettings settings)
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