using System.Threading.Tasks;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
///     Interface for clipboard operations.
///     Defines the contract for clipboard read/write operations with retry logic.
/// </summary>
public interface IClipboardManager
{
    /// <summary>
    ///     Gets the current text content from the clipboard.
    ///     Returns empty string on failure.
    /// </summary>
    /// <returns>The clipboard text, or empty string if unavailable.</returns>
    string GetText();

    /// <summary>
    ///     Sets text content to the clipboard.
    ///     Throws exception on failure.
    /// </summary>
    /// <param name="text">The text to set.</param>
    /// <exception cref="System.InvalidOperationException">Thrown when clipboard operation fails.</exception>
    void SetText(string text);

    /// <summary>
    ///     Gets text from clipboard with retry logic.
    ///     Useful after copy operations where clipboard may not be immediately available.
    /// </summary>
    /// <returns>The clipboard text, or empty string if all retries fail.</returns>
    Task<string> GetTextWithRetryAsync();

    /// <summary>
    ///     Restores the clipboard to the specified text.
    ///     Silently fails on error (for auto-recovery scenarios).
    /// </summary>
    /// <param name="text">The text to restore, or null/empty to clear clipboard.</param>
    void RestoreText(string text);
}