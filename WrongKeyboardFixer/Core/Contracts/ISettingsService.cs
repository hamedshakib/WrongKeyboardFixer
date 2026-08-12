namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for loading and saving application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Loads application settings from storage.
    /// </summary>
    AppSettings Load();

    /// <summary>
    /// Saves application settings to storage.
    /// </summary>
    void Save(AppSettings settings);

    /// <summary>
    /// Configures the application to run on system startup.
    /// </summary>
    void ConfigureStartup(bool enable);
}
