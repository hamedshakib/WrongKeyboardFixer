using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Platforms.Windows;

/// <summary>
/// Clipboard operations using MAUI Clipboard API.
/// Provides save/restore functionality with retry logic.
/// </summary>
public sealed class WindowsClipboardService : Core.Services.IClipboardService
{
    private const int MaxRetryAttempts = 8;
    private const int RetryDelayMs = 150;

    public string GetText()
    {
        try
        {
            if (Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText)
            {
                var task = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
                task.Wait(TimeSpan.FromSeconds(2));
                return task.Result ?? string.Empty;
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetTextWithRetryAsync()
    {
        string text = GetText();
        for (int attempt = 0; string.IsNullOrWhiteSpace(text) && attempt < MaxRetryAttempts; attempt++)
        {
            await Task.Delay(RetryDelayMs);
            text = GetText();
        }
        return text;
    }

    public void SetText(string text)
    {
        try
        {
            var task = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(text);
            task.Wait(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(Localization.Get("ClipboardSetTextError"), ex);
        }
    }

    public void RestoreText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            var task = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(text);
            task.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Silently fail
        }
    }
}
