namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for reading and writing text to the clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Gets the current text content of the clipboard.
    /// </summary>
    string GetText();

    /// <summary>
    /// Sets the text content of the clipboard.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Asynchronously gets the text content with retry logic.
    /// </summary>
    Task<string> GetTextWithRetryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores the clipboard to a previous state.
    /// </summary>
    void RestoreText(string? text);
}
