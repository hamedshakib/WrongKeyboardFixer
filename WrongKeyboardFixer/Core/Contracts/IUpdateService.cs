namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for checking and installing application updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks for available updates from the release source.
    /// </summary>
    Task<UpdateStatus> CheckForUpdatesAsync(
        IProgress<(int percent, string message)>? progress = null,
        bool silent = false);

    /// <summary>
    /// Gets the current version of the application.
    /// </summary>
    string GetCurrentVersionString();
}

/// <summary>
/// Represents the status of an update check.
/// </summary>
public enum UpdateStatus
{
    NoUpdate,
    UpdateAvailable,
    DownloadedAndInstalling,
    UserDeclined,
    Error
}
