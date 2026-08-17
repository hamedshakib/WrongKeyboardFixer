using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Persistence;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Loads and saves application settings to a JSON file.
/// Uses Environment.CommonApplicationDataPath for cross-platform compatibility.
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "WrongKeyboardFixer",
        "settings.json"
    );

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

            if (settings.PersianToEnglishMap is null || settings.PersianToEnglishMap.Count == 0)
                settings.PersianToEnglishMap = MappingDefaults.GetDefaultPersianToEnglishMap();

            if (settings.EnglishToPersianMap is null || settings.EnglishToPersianMap.Count == 0)
                settings.EnglishToPersianMap = MappingDefaults.GetDefaultEnglishToPersianMap();

            if (settings.WordCorrections is null)
                settings.WordCorrections = MappingDefaults.GetDefaultWordCorrections();

            return settings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load settings: {ex.Message}");
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
            Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public static void AddToStartup(bool enable)
    {
#if WINDOWS
        var registry = new Platforms.Windows.WindowsRegistryService();
        registry.SetAutoStart(enable);
#endif
    }
}
