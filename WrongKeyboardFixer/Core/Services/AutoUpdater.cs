using System.Diagnostics;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Orchestrates the update workflow: fetch the latest release, compare versions,
/// and hand the download/install work to the update services.
/// Uses events for user interaction (no direct UI dependency).
/// </summary>
public class AutoUpdater
{
    public enum UpdateStatus
    {
        NoUpdate,
        UpdateAvailable,
        DownloadedAndInstalling,
        UserDeclined,
        Error
    }

    private readonly GitHubReleaseClient Client = new();

    /// <summary>Raised when the user needs to confirm an action (Yes/No prompt).</summary>
    public event Func<string, string, Task<bool>>? ConfirmRequested;

    /// <summary>Raised when a message needs to be shown to the user.</summary>
    public event Action<string, string>? MessageRequested;

    /// <summary>Raised when the app needs to exit for update installation.</summary>
    public event Action? ExitRequested;

    public async Task<UpdateStatus> CheckForUpdatesAsync(
        IProgress<(int percent, string message)>? progress = null,
        bool silent = false)
    {
        try
        {
            progress?.Report((0, Localization.Get("UpdCheckingCurrent")));
            var currentVersion = VersionInfo.Current;

            progress?.Report((10, Localization.Get("UpdFetchingRelease")));
            var latestRelease = await Client.GetLatestReleaseAsync();
            if (latestRelease == null)
            {
                if (!silent)
                    MessageRequested?.Invoke(Localization.Get("UpdFetchError"), Localization.Get("Error"));
                return UpdateStatus.Error;
            }

            var latestVersion = VersionInfo.ParseTag(latestRelease.TagName);

            if (latestVersion <= currentVersion)
            {
                if (!silent)
                    MessageRequested?.Invoke(
                        Localization.Format("UpdNoUpdate", currentVersion),
                        Localization.Get("Update"));
                progress?.Report((100, Localization.Get("UpdNoUpdateShort")));
                return UpdateStatus.NoUpdate;
            }

            progress?.Report((15, Localization.Format("UpdFound", latestVersion)));

            if (ConfirmRequested != null)
            {
                bool accepted = await ConfirmRequested.Invoke(
                    Localization.Format("UpdAvailablePrompt", latestVersion, currentVersion),
                    Localization.Get("UpdAvailableTitle"));

                if (!accepted)
                    return UpdateStatus.UserDeclined;
            }
            else
            {
                return UpdateStatus.UserDeclined;
            }

            var installer = new UpdateInstaller(Client);
            bool restartScheduled = await installer.InstallAsync(latestRelease, latestVersion, progress);
            if (restartScheduled)
            {
                ExitRequested?.Invoke();
                return UpdateStatus.DownloadedAndInstalling;
            }

            return UpdateStatus.Error;
        }
        catch (UpdateInstallException ex)
        {
            Debug.WriteLine($"Update install failed: {ex.Message}");
            progress?.Report((100, ex.Message));
            if (!silent)
                MessageRequested?.Invoke(ex.Message, Localization.Get("Error"));
            return UpdateStatus.Error;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex.Message}");
            progress?.Report((100, Localization.Format("UpdCheckError", ex.Message)));
            if (!silent)
                MessageRequested?.Invoke(Localization.Format("UpdCheckError", ex.Message), Localization.Get("Error"));
            return UpdateStatus.Error;
        }
    }
}
