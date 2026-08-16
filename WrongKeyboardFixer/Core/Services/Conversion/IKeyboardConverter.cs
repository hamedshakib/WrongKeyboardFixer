using System.Collections.Generic;
using WrongKeyboardFixer.Core.Models;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
/// Defines the interface for keyboard conversion between Persian and English.
/// </summary>
public interface IKeyboardConverter
{
    /// <summary>
    /// Converts a Persian character to English using custom mappings or defaults.
    /// </summary>
    /// <param name="persianChar">The Persian character to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The English character, or null if no mapping exists.</returns>
    char? ConvertPersianToEnglish(char persianChar, IDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts a Persian string to English using custom mappings or defaults.
    /// </summary>
    /// <param name="text">The Persian text to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The English string.</returns>
    string ConvertPersianToEnglish(string? text, IDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts an English character to Persian using custom mappings or defaults.
    /// </summary>
    /// <param name="englishChar">The English character to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The Persian character, or null if no mapping exists.</returns>
    char? ConvertEnglishToPersian(char englishChar, IDictionary<char, char>? customMappings = null);

    /// <summary>
    /// Converts an English string to Persian using custom mappings or defaults.
    /// </summary>
    /// <param name="text">The English text to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <param name="wordCorrections">Optional collection of known words for context-aware conversion.</param>
    /// <returns>The Persian string.</returns>
    string ConvertEnglishToPersian(string? text, IDictionary<char, char>? customMappings = null, IReadOnlyCollection<string>? wordCorrections = null);

    /// <summary>
    /// Detects whether text is mostly English or Persian.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>True if text is mostly English (should convert to Persian), false otherwise.</returns>
    bool ShouldConvertToPersian(string? text);
}