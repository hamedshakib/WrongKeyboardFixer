using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Services.Update;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
///     Orchestrates the update workflow: fetch the latest release, compare versions,
///     ask the user, and hand the download/install work to the update services.
///     Owns all user-facing messaging for the flow.
/// </summary>
public static class AutoUpdater
{
    public enum UpdateStatus
    {
        NoUpdate,
        UpdateAvailable,
        DownloadedAndInstalling,
        UserDeclined,
        Error
    }

    // One shared client for all requests (connection reuse, no per-call setup).
    private static readonly GitHubReleaseClient Client = new();

    /// <summary>
    ///     Checks for updates from GitHub releases.
    /// </summary>
    /// <param name="progress">Optional progress reporter for download status.</param>
    /// <param name="silent">If true, suppresses the "no update" and "error" messages (startup mode).</param>
    /// <returns>The update status result.</returns>
    public static async Task<UpdateStatus> CheckForUpdatesAsync(
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
                    ShowMessage(Localization.Get("UpdFetchError"), Localization.Get("Error"), MessageBoxIcon.Error);
                return UpdateStatus.Error;
            }

            var latestVersion = VersionInfo.ParseTag(latestRelease.TagName);

            if (latestVersion <= currentVersion)
            {
                if (!silent)
                    ShowMessage(
                        Localization.Format("UpdNoUpdate", currentVersion),
                        Localization.Get("Update"),
                        MessageBoxIcon.Information);
                progress?.Report((100, Localization.Get("UpdNoUpdateShort")));
                return UpdateStatus.NoUpdate;
            }

            // Ask the user whether to download the new version.
            progress?.Report((15, Localization.Format("UpdFound", latestVersion)));
            var result = MessageBox.Show(
                Localization.Format("UpdAvailablePrompt", latestVersion, currentVersion),
                Localization.Get("UpdAvailableTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return UpdateStatus.UserDeclined;

            var installer = new UpdateInstaller(Client);
            bool restartScheduled = await installer.InstallAsync(latestRelease, latestVersion, progress);
            if (restartScheduled)
            {
                // Exit so the replacement script can swap the running exe.
                Application.Exit();
                return UpdateStatus.DownloadedAndInstalling;
            }

            return UpdateStatus.Error;
        }
        catch (UpdateInstallException ex)
        {
            Debug.WriteLine($"❌ Update install failed: {ex.Message}");
            progress?.Report((100, ex.Message));
            if (!silent)
                ShowMessage(ex.Message, Localization.Get("Error"), ex.IsWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
            return UpdateStatus.Error;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update check failed: {ex.Message}");
            progress?.Report((100, Localization.Format("UpdCheckError", ex.Message)));
            if (!silent)
                ShowMessage(Localization.Format("UpdCheckError", ex.Message), Localization.Get("Error"), MessageBoxIcon.Error);
            return UpdateStatus.Error;
        }
    }

    private static void ShowMessage(string text, string caption, MessageBoxIcon icon)
    {
        MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
    }
}