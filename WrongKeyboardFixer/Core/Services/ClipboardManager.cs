using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
///     Provides reliable clipboard operations with retry logic.
///     Implements IClipboardManager interface for testability.
/// </summary>
public class ClipboardManager : IClipboardManager
{
    private const int MaxRetryAttempts = 8;
    private const int RetryDelayMs = 150;

    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClipboardManager" /> class.
    /// </summary>
    /// <param name="logger">The logger for recording clipboard operations.</param>
    public ClipboardManager(ILogger? logger = null)
    {
        _logger = logger ?? new ConsoleLogger();
    }

    /// <summary>
    ///     Gets the current text content from the clipboard.
    ///     Returns empty string on failure.
    /// </summary>
    /// <returns>The clipboard text, or empty string if unavailable.</returns>
    public string GetText()
    {
        try
        {
            string? text = Clipboard.GetText(TextDataFormat.UnicodeText);
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.Warning("Failed to get clipboard text", ex);
            return string.Empty;
        }
    }

    /// <summary>
    ///     Sets text content to the clipboard.
    ///     Throws exception on failure.
    /// </summary>
    /// <param name="text">The text to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when clipboard operation fails.</exception>
    public void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to set clipboard text", ex);
            throw new InvalidOperationException(Localization.Get("ClipboardSetTextError"), ex);
        }
    }

    /// <summary>
    ///     Gets text from clipboard with retry logic.
    ///     Useful after copy operations where clipboard may not be immediately available.
    /// </summary>
    /// <returns>The clipboard text, or empty string if all retries fail.</returns>
    public async Task<string> GetTextWithRetryAsync()
    {
        _logger.Debug("Attempting to read clipboard with retry logic");

        // Read immediately first: the caller already waits after sending Ctrl+C,
        // so sleeping before the first attempt only adds needless latency.
        string text = GetText();
        int attempt = 0;

        while (string.IsNullOrWhiteSpace(text) && attempt < MaxRetryAttempts)
        {
            attempt++;
            _logger.Debug($"Clipboard read attempt {attempt}/{MaxRetryAttempts} failed, retrying...");
            await Task.Delay(RetryDelayMs);
            text = GetText();
        }

        if (string.IsNullOrWhiteSpace(text))
            _logger.Warning($"Clipboard read failed after {MaxRetryAttempts} attempts");
        else
            _logger.Debug($"Clipboard read succeeded on attempt {attempt + 1}");

        return text;
    }

    /// <summary>
    ///     Restores the clipboard to the specified text.
    ///     Silently fails on error (for auto-recovery scenarios).
    /// </summary>
    /// <param name="text">The text to restore, or null/empty to clear clipboard.</param>
    public void RestoreText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ClearClipboard();
            return;
        }

        try
        {
            Clipboard.SetText(text);
            _logger.Debug("✅ Clipboard restored");
        }
        catch (Exception ex)
        {
            _logger.Warning("Failed to restore clipboard text", ex);
            // Silently fail for auto-recovery scenarios
        }
    }

    /// <summary>
    ///     Clears the clipboard content.
    ///     Used when restoring null/empty text.
    /// </summary>
    private void ClearClipboard()
    {
        try
        {
            Clipboard.Clear();
            var emptyData = new DataObject();
            emptyData.SetData(DataFormats.Text, string.Empty);
            Clipboard.SetDataObject(emptyData, true);
            _logger.Debug("🧹 Clipboard cleared");
        }
        catch (Exception ex)
        {
            _logger.Warning("Failed to clear clipboard", ex);
            // Silently fail for auto-recovery scenarios
        }
    }
}