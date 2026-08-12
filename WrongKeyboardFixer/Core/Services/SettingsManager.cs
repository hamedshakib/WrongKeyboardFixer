using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Contracts;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Manages application settings persistence and startup configuration.
/// </summary>
public sealed class SettingsManager : ISettingsService
{
    private readonly string _settingsPath;

    public SettingsManager()
    {
        _settingsPath = Path.Combine(
            Application.CommonAppDataPath,
            "WrongKeyboardFixer",
            "settings.json"
        );
    }

    /// <summary>
    /// Loads application settings from storage.
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();

            string json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings) 
                ?? new AppSettings();

            // Initialize custom mappings from defaults if empty
            if (settings.PersianToEnglishMap.Count == 0)
            {
                settings.PersianToEnglishMap = MappingDefaults.GetDefaultPersianToEnglishMap();
            }

            if (settings.EnglishToPersianMap.Count == 0)
            {
                settings.EnglishToPersianMap = MappingDefaults.GetDefaultEnglishToPersianMap();
            }

            return settings;
        }
        catch
        {
            // In case of error, return default settings
            return new AppSettings();
        }
    }

    /// <summary>
    /// Saves application settings to storage.
    /// </summary>
    public void Save(AppSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        try
        {
            string? directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(settings, AppSettingsJsonContext.Default.AppSettings);
            File.WriteAllText(_settingsPath, json);
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

    /// <summary>
    /// Configures the application to run on system startup.
    /// </summary>
    public void ConfigureStartup(bool enable)
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