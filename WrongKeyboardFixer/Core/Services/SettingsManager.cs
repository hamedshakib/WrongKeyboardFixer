using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Persistence;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Loads and saves application settings to a JSON file, and manages
/// the Windows auto-start registry entry.
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Application.CommonAppDataPath,
        "WrongKeyboardFixer",
        "settings.json"
    );

    private static readonly RegistryManager RegistryManager = new();

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return CreateDefaultSettings();

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);

            if (settings == null)
                return CreateDefaultSettings();

            // Initialize custom mappings from defaults if empty
            if (settings.PersianToEnglishMap is null || settings.PersianToEnglishMap.Count == 0)
                settings.PersianToEnglishMap = MappingDefaults.GetDefaultPersianToEnglishMap();

            if (settings.EnglishToPersianMap is null || settings.EnglishToPersianMap.Count == 0)
                settings.EnglishToPersianMap = MappingDefaults.GetDefaultEnglishToPersianMap();

            return settings;
        }
        catch (Exception ex)
        {
            // On any failure, return fresh default settings
            Debug.WriteLine($"⚠️ Failed to load settings: {ex.Message}");
            return CreateDefaultSettings();
        }
    }

    private static AppSettings CreateDefaultSettings() => new();

    public static void Save(AppSettings settings)
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
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
        RegistryManager.SetAutoStart(enable);
    }
}
