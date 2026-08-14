using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
/// Communicates with the GitHub Releases API and downloads release assets.
/// Owns a single shared <see cref="HttpClient"/> for connection reuse.
/// </summary>
internal sealed class GitHubReleaseClient : IDisposable
{
    private const string GitHubApiUrl = "https://api.github.com/repos/hamedshakib/WrongKeyboardFixer/releases/latest";

    private readonly HttpClient _http;
    private bool _disposed;

    public GitHubReleaseClient()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("WrongKeyboardFixer-AutoUpdater");
    }

    /// <summary>
    /// Fetches the latest release metadata from GitHub. Returns null when the
    /// request fails or no downloadable asset (zip/exe) is present.
    /// </summary>
    public async Task<ReleaseInfo?> GetLatestReleaseAsync()
    {
        try
        {
            using var response = await _http.GetAsync(GitHubApiUrl);
            response.EnsureSuccessStatusCode();

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = json.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagNameElement) ||
                !root.TryGetProperty("assets", out var assetsElement))
                return null;

            string? downloadUrl = null;
            foreach (var asset in assetsElement.EnumerateArray())
            {
                if (!asset.TryGetProperty("browser_download_url", out var urlElement) ||
                    !asset.TryGetProperty("name", out var nameElement))
                    continue;

                string name = nameElement.GetString() ?? "";
                if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = urlElement.GetString();
                    break;
                }
            }

            return downloadUrl != null
                ? new ReleaseInfo(tagNameElement.GetString() ?? "", downloadUrl)
                : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to get latest release: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns an open stream for downloading the given release asset.
    /// Caller is responsible for disposing the response.
    /// </summary>
    public Task<HttpResponseMessage> GetAssetAsync(string downloadUrl)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _http.Dispose();
    }
}
