using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Repositories;

/// <summary>
/// File-based implementation of ISettingsRepository
/// </summary>
public class SettingsFileRepository : ISettingsRepository
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SettingsFileRepository()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WrongKeyboardFixer",
            "settings.json"
        );

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();

            string json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            return settings ?? new AppSettings();
        }
        catch
        {
            // در صورت خطا، تنظیمات پیش‌فرض برگردانده می‌شود
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            // ایجاد پوشه اگر وجود نداشته باشد
            string directory = Path.GetDirectoryName(_settingsPath)!;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(_settingsPath, json);
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

    public void AddToStartup(bool enable)
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
                $"خطا در تنظیم اجرای خودکار: {ex.Message}",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}
