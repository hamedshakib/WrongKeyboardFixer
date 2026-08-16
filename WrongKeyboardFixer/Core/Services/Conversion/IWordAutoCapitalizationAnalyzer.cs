namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
/// Defines the interface for analyzing Word auto-capitalization patterns.
/// </summary>
public interface IWordAutoCapitalizationAnalyzer
{
    /// <summary>
    /// Determines whether an uppercase first letter may have been created by Word auto-capitalization.
    /// </summary>
    /// <param name="text">The full text.</param>
    /// <param name="wordStart">The starting index of the word in the text.</param>
    /// <param name="word">The word to analyze.</param>
    /// <returns>True if the word was likely auto-capitalized by Word.</returns>
    bool IsAutoCapitalizedByWord(string text, int wordStart, string word);

    /// <summary>
    /// Detects Word's standalone "i" to "I" correction at any position.
    /// </summary>
    /// <param name="text">The full text.</param>
    /// <param name="index">The index of the character to check.</param>
    /// <returns>True if the character at index is a standalone "I" that was auto-lowercased.</returns>
    bool IsAutoLowercaseIAt(string text, int index);
}