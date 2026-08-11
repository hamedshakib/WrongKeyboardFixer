using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

public static class AutoUpdater
{
    private const string GitHubApiUrl = "https://api.github.com/repos/hamedshakib/WrongKeyboardFixer/releases/latest";

    public enum UpdateStatus
    {
        NoUpdate,
        UpdateAvailable,
        DownloadedAndInstalling,
        UserDeclined,
        Error
    }

    /// <summary>
    /// Checks for updates from GitHub releases.
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
            var currentVersion = GetCurrentVersion();

            progress?.Report((10, Localization.Get("UpdFetchingRelease")));
            var latestRelease = await GetLatestReleaseInfoAsync();
            if (latestRelease == null)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        Localization.Get("UpdFetchError"),
                        Localization.Get("Error"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                return UpdateStatus.Error;
            }

            var latestVersion = ParseVersion(((JsonElement)latestRelease).GetProperty("tag_name").GetString());

            var current = Version.Parse(currentVersion);

            if (latestVersion <= current)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        Localization.Format("UpdNoUpdate", currentVersion),
                        Localization.Get("Update"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                progress?.Report((100, Localization.Get("UpdNoUpdateShort")));
                return UpdateStatus.NoUpdate;
            }

            // Show update available dialog
            progress?.Report((15, Localization.Format("UpdFound", latestVersion)));
            var result = MessageBox.Show(
                Localization.Format("UpdAvailablePrompt", latestVersion, currentVersion),
                Localization.Get("UpdAvailableTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return UpdateStatus.UserDeclined;

            return await DownloadAndInstallUpdateAsync(latestRelease, latestVersion, progress);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update check failed: {ex.Message}");
            progress?.Report((100, Localization.Format("UpdCheckError", ex.Message)));
            if (!silent)
            {
                MessageBox.Show(
                    Localization.Format("UpdCheckError", ex.Message),
                    Localization.Get("Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return UpdateStatus.Error;
        }
    }

    private static string GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }

    private static async Task<object?> GetLatestReleaseInfoAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var response = await client.GetStringAsync(GitHubApiUrl);
            using var doc = JsonDocument.Parse(response);
            return doc.RootElement;
        }
        catch
        {
            return null;
        }
    }

    private static Version ParseVersion(string versionString)
    {
        return Version.TryParse(versionString.TrimStart('v'), out var version) ? version : new Version(0, 0);
    }

    private static async Task<UpdateStatus> DownloadAndInstallUpdateAsync(
        object latestRelease,
        Version latestVersion,
        IProgress<(int percent, string message)>? progress)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            // Get download URL from latest release
            string? downloadUrl = null;
            if (latestRelease is JsonElement release)
            {
                var assets = release.GetProperty("assets");
                foreach (var asset in assets.EnumerateArray())
                {
                    string? browserDownloadUrl = asset.GetProperty("browser_download_url").GetString();
                    if (!string.IsNullOrEmpty(browserDownloadUrl) && browserDownloadUrl.EndsWith(".zip"))
                    {
                        downloadUrl = browserDownloadUrl;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(downloadUrl))
            {
                throw new InvalidOperationException(Localization.Get("UpdExeNotFound"));
            }

            progress?.Report((20, Localization.Get("UpdDownloading")));
            
            var zipBytes = await client.GetByteArrayAsync(downloadUrl);
            progress?.Report((60, Localization.Get("UpdPreparing")));

            string tempFolder = Path.Combine(Path.GetTempPath(), "WrongKeyboardFixerUpdate");
            Directory.CreateDirectory(tempFolder);
            Directory.Delete(tempFolder, true);
            Directory.CreateDirectory(tempFolder);

            string zipPath = Path.Combine(tempFolder, "update.zip");
            File.WriteAllBytes(zipPath, zipBytes);

            ZipFile.ExtractToDirectory(zipPath, tempFolder);

            // Find the EXE in the extracted folder
            string exeFile = Directory.GetFiles(tempFolder, "*.exe", SearchOption.AllDirectories)
                .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains("WrongKeyboardFixer"));

            if (exeFile == null || !File.Exists(exeFile))
            {
                Directory.Delete(tempFolder, true);
                throw new InvalidOperationException(Localization.Get("UpdExeNotFound"));
            }

            var newVersion = FileVersionInfo.GetVersionInfo(exeFile).ProductVersion;
            var current = Version.Parse(GetCurrentVersion());
            var newVer = Version.Parse(newVersion);

            if (current >= newVer)
            {
                Directory.Delete(tempFolder, true);
                throw new InvalidOperationException(Localization.Format("UpdNotNewer", newVer, current));
            }

            progress?.Report((80, Localization.Get("UpdInstalling")));

            string appPath = Application.ExecutablePath;
            string tempExe = Path.Combine(tempFolder, "setup.exe");

            // Create a simple installer batch file
            string batchFile = Path.Combine(tempFolder, "install.bat");
            string batchContent = $"@echo off\n" +
                $"timeout /t 2 /nobreak >nul\n" +
                $"copy \"{exeFile}\" \"{appPath}\"\n" +
                $"start \"\" \"{appPath}\"\n" +
                $"rd /s /q \"{tempFolder}\"\n" +
                $"exit";

            File.WriteAllText(batchFile, batchContent);
            Process.Start(batchFile);

            Application.Exit();
            return UpdateStatus.DownloadedAndInstalling;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update download/install failed: {ex.Message}");
            progress?.Report((100, Localization.Format("UpdDownloadInstallError", ex.Message)));
            MessageBox.Show(
                Localization.Format("UpdDownloadInstallError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return UpdateStatus.Error;
        }
    }
}