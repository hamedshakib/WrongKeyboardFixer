using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
/// Performs the mechanical work of applying an update: downloading the release
/// asset, extracting it, verifying the new version and launching the batch
/// script that swaps the running executable.
/// Problems are reported by throwing <see cref="UpdateInstallException"/> so the
/// caller decides how to present them; this class never touches the UI.
/// </summary>
internal sealed class UpdateInstaller
{
    private const int DownloadBufferSize = 65536; // 64 KiB – fewer syscalls, faster downloads
    private const int CopyBufferSize = 8192;

    private readonly GitHubReleaseClient _client;

    public UpdateInstaller(GitHubReleaseClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Downloads and installs the given release. On success starts the
    /// replacement script and returns true.
    /// Throws <see cref="UpdateInstallException"/> when the update cannot be applied.
    /// </summary>
    public async Task<bool> InstallAsync(
        ReleaseInfo release,
        Version newVersion,
        IProgress<(int percent, string message)>? progress)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"WrongKeyboardFixer_Update_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            bool isZip = release.DownloadUrl.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            string downloadedFile = Path.Combine(tempDir, isZip ? "update.zip" : "update.exe");

            progress?.Report((20, Localization.Get("UpdDownloading")));
            await DownloadAssetAsync(release.DownloadUrl, downloadedFile, progress);

            progress?.Report((90, Localization.Get("UpdPreparing")));

            string currentExePath = Environment.ProcessPath ?? throw new InvalidOperationException("Unable to determine executable path.");
            string? newExePath = isZip
                ? await ExtractZipAsync(downloadedFile, tempDir)
                : downloadedFile;

            if (newExePath == null)
                throw new UpdateInstallException(Localization.Get("UpdExeNotFound"));

            var newExeVersion = VersionInfo.FromFile(newExePath);
            if (newExeVersion != null && newExeVersion <= VersionInfo.Current)
                throw new UpdateInstallException(
                    Localization.Format("UpdNotNewer", newExeVersion, VersionInfo.Current),
                    isWarning: true);

            string batchPath = Path.Combine(tempDir, "update.bat");
            File.WriteAllText(batchPath, BatchScriptGenerator.Generate(
                processId: Environment.ProcessId,
                newExePath: newExePath,
                currentExePath: currentExePath,
                tempDir: tempDir));

            progress?.Report((95, Localization.Get("UpdInstalling")));

            Process.Start(new ProcessStartInfo
            {
                FileName = batchPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            });

            progress?.Report((100, Localization.Get("UpdInstallComplete")));
            return true;
        }
        catch (UpdateInstallException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Download/install failed: {ex.Message}");
            throw new UpdateInstallException(Localization.Format("UpdDownloadInstallError", ex.Message));
        }
        // Note: tempDir must NOT be deleted here — the batch script needs it
        // (update.bat / update.exe / extracted files) to perform the swap, and
        // it removes the directory itself after the application restarts.
    }

    /// <summary>Downloads the asset to <paramref name="destFile"/> with progress reporting.</summary>
    private async Task DownloadAssetAsync(
        string downloadUrl,
        string destFile,
        IProgress<(int percent, string message)>? progress)
    {
        using var response = await _client.GetAssetAsync(downloadUrl);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(DownloadBufferSize);
        try
        {
            int bytesRead;
            long totalRead = 0;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    int percent = (int)(20 + (totalRead * 70.0 / totalBytes));
                    progress?.Report((Math.Min(percent, 90), Localization.Format("UpdDownloadPercent", percent)));
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>Extracts a zip into <paramref name="tempDir"/>/extracted and locates the exe inside.</summary>
    private static async Task<string?> ExtractZipAsync(string zipPath, string tempDir)
    {
        string extractDir = Path.Combine(tempDir, "extracted");
        Directory.CreateDirectory(extractDir);

        using var archive = new ZipArchive(File.OpenRead(zipPath), ZipArchiveMode.Read);

        byte[] buffer = ArrayPool<byte>.Shared.Rent(CopyBufferSize);
        try
        {
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

                // Security check: prevent zip slip (path traversal) — resolve the
                // full path and verify it stays inside the extraction directory.
                string fullDestPath = Path.GetFullPath(destPath);
                string fullExtractDir = Path.GetFullPath(extractDir);
                if (!fullDestPath.StartsWith(fullExtractDir, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"⚠️ Skipping potentially malicious entry: {entry.FullName}");
                    continue;
                }

                string? destDir = Path.GetDirectoryName(destPath);
                if (destDir != null)
                    Directory.CreateDirectory(destDir);

                await using var entryStream = entry.Open();
                await using var destFileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                await CopyToAsync(entryStream, destFileStream, buffer);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return FindExecutable(extractDir);
    }

    private static async Task CopyToAsync(Stream source, Stream destination, byte[] buffer)
    {
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer)) > 0)
            await destination.WriteAsync(buffer, 0, bytesRead);
    }

    /// <summary>Recursively searches for the first .exe file in a directory.</summary>
    private static string? FindExecutable(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*.exe", SearchOption.AllDirectories))
            return file;
        return null;
    }
}
