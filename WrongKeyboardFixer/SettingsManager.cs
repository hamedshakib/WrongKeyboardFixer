using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Application.CommonAppDataPath,
        "PersianKeyboardFix",
        "settings.json"
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            // در صورت خطا، تنظیمات پیش‌فرض برگردانده می‌شود
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            // ایجاد پوشه اگر وجود نداشته باشد
            string directory = Path.GetDirectoryName(SettingsPath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در ذخیره تنظیمات: {ex.Message}",
                "خطا",
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
                key.SetValue("PersianKeyboardFix", $"\"{appPath}\"");
            }
            else
            {
                if (key.GetValue("PersianKeyboardFix") != null)
                    key.DeleteValue("PersianKeyboardFix");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در تنظیم اجرای خودکار: {ex.Message}",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}