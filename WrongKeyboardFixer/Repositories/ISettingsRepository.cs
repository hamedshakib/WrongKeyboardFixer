using System;

namespace WrongKeyboardFixer.Repositories;

/// <summary>
/// Repository interface for managing application settings
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Load settings from storage
    /// </summary>
    /// <returns>Application settings</returns>
    AppSettings Load();

    /// <summary>
    /// Save settings to storage
    /// </summary>
    /// <param name="settings">Settings to save</param>
    void Save(AppSettings settings);

    /// <summary>
    /// Configure application to run on startup
    /// </summary>
    /// <param name="enable">Enable or disable startup</param>
    void AddToStartup(bool enable);
}
