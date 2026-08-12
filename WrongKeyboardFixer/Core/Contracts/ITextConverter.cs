namespace WrongKeyboardFixer.Core.Contracts;

/// <summary>
/// Provides methods for converting text between Persian and English keyboard layouts.
/// </summary>
public interface ITextConverter
{
    /// <summary>
    /// Converts a single Persian character to English.
    /// </summary>
    char? ConvertPersianToEnglish(char persianChar, IReadOnlyDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts a single English character to Persian.
    /// </summary>
    char? ConvertEnglishToPersian(char englishChar, IReadOnlyDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts text from Persian to English.
    /// </summary>
    string ConvertPersianToEnglish(string text, IReadOnlyDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts text from English to Persian.
    /// </summary>
    string ConvertEnglishToPersian(string text, IReadOnlyDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Detects if text should be converted to Persian based on character analysis.
    /// </summary>
    bool ShouldConvertToPersian(string text);
}
