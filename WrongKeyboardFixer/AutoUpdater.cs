using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

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
            progress?.Report((0, "بررسی نسخه فعلی..."));
            var currentVersion = GetCurrentVersion();

            progress?.Report((10, "دریافت اطلاعات آخرین نسخه از GitHub..."));
            var latestRelease = await GetLatestReleaseInfoAsync();
            if (latestRelease == null)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        "خطا در دریافت اطلاعات بروزرسانی از سرور.\nلطفاً اتصال اینترنت خود را بررسی کنید.",
                        "خطا",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                return UpdateStatus.Error;
            }

            var latestVersion = ParseVersion(latestRelease.TagName);

            if (latestVersion <= currentVersion)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        $"شما از آخرین نسخه استفاده می‌کنید.\n\nنسخه فعلی: {currentVersion}",
                        "بروزرسانی",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                progress?.Report((100, "شما از آخرین نسخه استفاده می‌کنید"));
                return UpdateStatus.NoUpdate;
            }

            // Show update available dialog
            progress?.Report((15, $"نسخه جدید {latestVersion} یافت شد"));
            var result = MessageBox.Show(
                $"نسخه جدید {latestVersion} منتشر شده است.\n" +
                $"نسخه فعلی: {currentVersion}\n\n" +
                $"آیا می‌خواهید آن را دانلود و نصب کنید؟",
                "بروزرسانی موجود است",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return UpdateStatus.UserDeclined;

            return await DownloadAndInstallUpdateAsync(latestRelease, latestVersion, progress);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update check failed: {ex.Message}");
            progress?.Report((100, $"خطا: {ex.Message}"));
            if (!silent)
            {
                MessageBox.Show(
                    $"خطا در بررسی بروزرسانی:\n{ex.Message}",
                    "خطا",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            return UpdateStatus.Error;
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

    public static string GetCurrentVersionString()
    {
        return GetCurrentVersion().ToString();
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
                        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                            name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
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

    private static async Task<UpdateStatus> DownloadAndInstallUpdateAsync(
        ReleaseInfo release,
        Version latestVersion,
        IProgress<(int percent, string message)>? progress)
    {
        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"WrongKeyboardFixer_Update_{release.TagName}");
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            string downloadUrl = release.DownloadUrl;
            bool isZip = downloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            string downloadedFile = Path.Combine(tempDir, isZip ? "update.zip" : "update.exe");

            // Download with progress
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WrongKeyboardFixer-AutoUpdater");

            progress?.Report((20, "در حال دانلود بروزرسانی..."));

            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(downloadedFile, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[8192];
            int bytesRead;
            long totalRead = 0;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    int percent = (int)(20 + (totalRead * 70.0 / totalBytes));
                    progress?.Report((Math.Min(percent, 90), $"دانلود: {percent}%"));
                }
            }

            progress?.Report((90, "در حال آماده‌سازی نصب..."));

            string currentExePath = Application.ExecutablePath;
            string? newExePath = null;

            if (isZip)
            {
                // Extract zip
                string extractDir = Path.Combine(tempDir, "extracted");
                Directory.CreateDirectory(extractDir);

                using var archive = new ZipArchive(File.OpenRead(downloadedFile), ZipArchiveMode.Read);
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries
                    if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
                    {
                        string dirPath = Path.Combine(extractDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(dirPath);
                        continue;
                    }

                    string destPath = Path.Combine(extractDir, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
                    string? destDir = Path.GetDirectoryName(destPath);
                    if (destDir != null)
                        Directory.CreateDirectory(destDir);

                    // Security check: prevent zip slip (path traversal)
                    string fullDestPath = Path.GetFullPath(destPath);
                    string fullExtractDir = Path.GetFullPath(extractDir);
                    if (!fullDestPath.StartsWith(fullExtractDir, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"⚠️ Skipping potentially malicious entry: {entry.FullName}");
                        continue;
                    }

                    using var entryStream = entry.Open();
                    using var destFileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                    await entryStream.CopyToAsync(destFileStream);
                }

                // Find the exe in extracted files
                newExePath = FindExecutable(extractDir);
                if (newExePath == null)
                {
                    MessageBox.Show(
                        "فایل اجرایی در فایل فشرده پیدا نشد.",
                        "خطا",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return UpdateStatus.Error;
                }
            }
            else
            {
                newExePath = downloadedFile;
            }

            // Create batch script to replace the running exe
            string batchPath = Path.Combine(tempDir, "update.bat");
            string batchContent = GenerateBatchScript(
                processId: Environment.ProcessId,
                newExePath: newExePath,
                currentExePath: currentExePath,
                tempDir: tempDir);
            File.WriteAllText(batchPath, batchContent);

            progress?.Report((95, "در حال نصب... برنامه مجدداً اجرا می‌شود"));

            // Start the batch script (hidden window)
            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });

            progress?.Report((100, "نصب کامل شد. برنامه مجدداً اجرا می‌شود."));

            // Exit current application so the batch script can replace the exe
            Application.Exit();
            return UpdateStatus.DownloadedAndInstalling;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Download/install failed: {ex.Message}");
            progress?.Report((100, $"خطا: {ex.Message}"));
            MessageBox.Show(
                $"خطا در دانلود یا نصب بروزرسانی:\n{ex.Message}",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return UpdateStatus.Error;
        }
    }

    /// <summary>
    /// Generates a batch script that waits for the current process to exit,
    /// replaces the executable, and restarts the application.
    /// </summary>
    private static string GenerateBatchScript(
        int processId,
        string newExePath,
        string currentExePath,
        string tempDir)
    {
        return $@"@echo off
chcp 65001 >nul
setlocal

:: Wait for the current application to exit
:waitloop
tasklist /fi ""PID eq {processId}"" 2>nul | find ""{processId}"" >nul
if %errorlevel% equ 0 (
    ping 127.0.0.1 -n 2 >nul
    goto waitloop
)

:: Copy the new executable over the old one
copy /y ""{newExePath}"" ""{currentExePath}"" >nul 2>&1

:: Start the updated application
start """" ""{currentExePath}""

:: Wait a moment for the app to start
ping 127.0.0.1 -n 3 >nul

:: Clean up temp directory
rd /s /q ""{tempDir}"" 2>nul

:: Delete this batch file
(goto) 2>nul & del ""%~f0""
";
    }

    /// <summary>
    /// Recursively searches for the first .exe file in a directory.
    /// </summary>
    private static string? FindExecutable(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.exe", SearchOption.AllDirectories))
        {
            return file;
        }
        return null;
    }

    private class ReleaseInfo
    {
        public string TagName { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
    }
}