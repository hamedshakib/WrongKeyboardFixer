using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace WrongKeyboardFixer;

public static class AutoUpdater
{
    private const string GitHubApiUrl = "https://api.github.com/repos/hamedshakib/WrongKeyboardFixer/releases/latest";
    public static async Task<bool> CheckForUpdatesAsync()
    {
        try
        {
            var currentVersion = GetCurrentVersion();
            var latestRelease = await GetLatestReleaseInfoAsync();
            if (latestRelease == null)
                return false;
            var latestVersion = ParseVersion(latestRelease.TagName);
            if (latestVersion > currentVersion)
            {
                return await DownloadAndInstallUpdateAsync(latestRelease, latestVersion);
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update check failed: {ex.Message}");
            return false;
        }
    }
    private static Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (versionAttribute != null && Version.TryParse(versionAttribute.InformationalVersion, out var version))
            return version;
        return assembly.GetName().Version ?? new Version(1, 0, 0);
    }
    private static async Task<ReleaseInfo?> GetLatestReleaseInfoAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WrongKeyboardFixer-AutoUpdater");
        try
        {
            var response = await httpClient.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var releaseData = JsonDocument.Parse(json);
            if (releaseData.RootElement.TryGetProperty("tag_name", out var tagNameElement) &&
                releaseData.RootElement.TryGetProperty("assets", out var assetsElement))
            {
                string? downloadUrl = null;
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.TryGetProperty("browser_download_url", out var urlElement) &&
                        asset.TryGetProperty("name", out var nameElement))
                    {
                        string name = nameElement.GetString() ?? "";
                        if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = urlElement.GetString();
                            break;
                        }
                        
                    }
                }
                if (downloadUrl != null)
                {
                    return new ReleaseInfo
                    {
                        TagName = tagNameElement.GetString() ?? "",
                        DownloadUrl = downloadUrl
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to get latest release: {ex.Message}");
        }
        return null;
    }
    private static Version ParseVersion(string tagName)
    {
        // Remove 'v' prefix if present
        string versionString = tagName.TrimStart('v');
        if (Version.TryParse(versionString, out var version))
            return version;
        return new Version(1, 0, 0);
    }
    private static async Task<bool> DownloadAndInstallUpdateAsync(ReleaseInfo release, Version latestVersion)
    {
        try
        {
            var result = MessageBox.Show(
                $"نسخه جدید {latestVersion} منتشر شده است.\nآیا می‌خواهید آن را دانلود و نصب کنید؟",
                "بروزرسانی موجود است",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return false;
            string tempPath = Path.Combine(Path.GetTempPath(), $"WrongKeyboardFixer_{release.TagName}.exe");
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WrongKeyboardFixer-AutoUpdater");
            var downloadBytes = await httpClient.GetByteArrayAsync(release.DownloadUrl);
            await File.WriteAllBytesAsync(tempPath, downloadBytes);
            // Start the installer and exit current application
            var startInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            };
            Process.Start(startInfo);
            // Schedule current app to close after a short delay
            Application.Exit();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Download/install failed: {ex.Message}");
            MessageBox.Show(
                $"خطا در دانلود یا نصب بروزرسانی:\n{ex.Message}",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }
    }
    private class ReleaseInfo
    {
        public string TagName { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
    }
}