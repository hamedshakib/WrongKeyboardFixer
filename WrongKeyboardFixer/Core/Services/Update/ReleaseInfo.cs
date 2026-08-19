namespace WrongKeyboardFixer.Core.Services.Update;

/// <summary>
///     Describes a downloadable release fetched from the GitHub Releases API.
/// </summary>
internal sealed class ReleaseInfo
{
    public ReleaseInfo(string tagName, string downloadUrl)
    {
        TagName = tagName;
        DownloadUrl = downloadUrl;
    }

    /// <summary>Release tag, e.g. "v1.3.0".</summary>
    public string TagName { get; }

    /// <summary>Direct URL of the release asset (zip or exe).</summary>
    public string DownloadUrl { get; }
}