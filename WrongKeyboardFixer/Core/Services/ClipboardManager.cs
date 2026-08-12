using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Contracts;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Manages clipboard operations with retry logic and error handling.
/// </summary>
public sealed class ClipboardManager : IClipboardService
{
    private const int MaxRetryAttempts = 8;
    private const int RetryDelayMs = 150;

    /// <summary>
    /// Gets the current text content of the clipboard.
    /// </summary>
    public string GetText()
    {
        try
        {
            return Clipboard.GetText(TextDataFormat.UnicodeText) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Sets the text content of the clipboard.
    /// </summary>
    public void SetText(string text)
    {
        if (string.IsNullOrEmpty(text)) throw new ArgumentException("Text cannot be null or empty.", nameof(text));

        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(Localization.Get("ClipboardSetTextError"), ex);
        }
    }

    /// <summary>
    /// Asynchronously gets the text content with retry logic.
    /// </summary>
    public async Task<string> GetTextWithRetryAsync(CancellationToken cancellationToken = default)
    {
        for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            await Task.Delay(RetryDelayMs, cancellationToken);

            string text = GetText();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        return string.Empty;
    }

    /// <summary>
    /// Restores the clipboard to a previous state.
    /// </summary>
    public void RestoreText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ClearClipboard();
            return;
        }

        try
        {
            Clipboard.SetText(text);
            Debug.WriteLine("✅ Clipboard restored");
        }
        catch
        {
            // Silently fail
        }
    }

    private void ClearClipboard()
    {
        try
        {
            Clipboard.Clear();
            var emptyData = new DataObject();
            emptyData.SetData(DataFormats.Text, string.Empty);
            Clipboard.SetDataObject(emptyData, true);
            Debug.WriteLine("🧹 Clipboard cleared");
        }
        catch
        {
            // Silently fail
        }
    }
}