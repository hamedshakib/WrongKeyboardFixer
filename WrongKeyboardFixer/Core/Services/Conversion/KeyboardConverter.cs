using System;
using System.Collections.Generic;
using System.Text;
using WrongKeyboardFixer.Core.Models;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
///     Converts characters between Persian and English based on user-defined mappings.
///     The English-to-Persian conversion includes a heuristic for Word-like auto-capitalization.
/// </summary>
public sealed class KeyboardConverter : IKeyboardConverter
{
    private static readonly HashSet<string> EmptyCorrections = new(StringComparer.Ordinal);

    private static readonly HashSet<string> DefaultWordCorrections =
        BuildCorrectionSet(MappingDefaults.GetDefaultWordCorrections());

    private readonly WordAutoCapitalizationAnalyzer _autoCapAnalyzer = new();

    private readonly TextDirectionDetector _directionDetector = new();
    private readonly PersianWordMatcher _wordMatcher = new();

    /// <summary>
    ///     Converts a Persian character to English using custom mappings or defaults.
    /// </summary>
    /// <param name="persianChar">The Persian character to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The English character, or null if no mapping exists.</returns>
    public char? ConvertPersianToEnglish(char persianChar, IDictionary<char, char>? customMappings = null)
    {
        if (customMappings is not null && customMappings.TryGetValue(persianChar, out var englishChar))
            return englishChar;

        if (MappingDefaults.GetDefaultPersianToEnglishMap().TryGetValue(persianChar, out var defaultEnglishChar))
            return defaultEnglishChar;

        return null;
    }

    /// <summary>
    ///     Converts an English character to Persian using custom mappings or defaults.
    /// </summary>
    /// <param name="englishChar">The English character to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The Persian character, or null if no mapping exists.</returns>
    public char? ConvertEnglishToPersian(char englishChar, IDictionary<char, char>? customMappings = null)
    {
        return ConvertPlainChar(englishChar, customMappings);
    }

    /// <summary>
    ///     Converts a Persian string to English using custom mappings or defaults.
    /// </summary>
    /// <param name="text">The Persian text to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <returns>The English string.</returns>
    public string ConvertPersianToEnglish(string? text, IDictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var sb = new StringBuilder(text.Length);

        foreach (char c in text)
        {
            char? mapped = ConvertPersianToEnglish(c, customMappings);
            sb.Append(mapped ?? c);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Converts an English string to Persian using custom mappings or defaults.
    ///     Uppercase first letters are resolved with a Word auto-capitalization heuristic.
    /// </summary>
    /// <param name="text">The English text to convert.</param>
    /// <param name="customMappings">Optional custom mappings.</param>
    /// <param name="wordCorrections">Optional collection of known words for context-aware conversion.</param>
    /// <returns>The Persian string.</returns>
    public string ConvertEnglishToPersian(
        string? text,
        IDictionary<char, char>? customMappings = null,
        IReadOnlyCollection<string>? wordCorrections = null)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        HashSet<string> corrections = wordCorrections is null
            ? DefaultWordCorrections
            : BuildCorrectionSet(wordCorrections);

        var sb = new StringBuilder(text.Length);
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (IsConversionWordChar(c, customMappings))
            {
                int start = i;

                while (i < text.Length && IsConversionWordChar(text[i], customMappings))
                    i++;

                string word = text.Substring(start, i - start);
                sb.Append(ConvertWordWithContext(text, start, word, customMappings, corrections));
            }
            else
            {
                sb.Append(ConvertPlainChar(c, customMappings) ?? c);
                i++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Analyzes the text and determines if it should be converted to Persian.
    ///     Returns true if the text is mostly English (should convert to Persian),
    ///     or false if it's mostly Persian (should convert to English).
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>True if text should be converted TO Persian, false otherwise.</returns>
    public bool ShouldConvertToPersian(string? text)
    {
        return _directionDetector.ShouldConvertToPersian(text);
    }

    /// <summary>
    ///     Converts one English word with attention to Word auto-capitalization context.
    /// </summary>
    private string ConvertWordWithContext(
        string text,
        int wordStart,
        string word,
        IDictionary<char, char>? customMappings,
        HashSet<string> corrections)
    {
        if (word.Length == 0)
            return string.Empty;

        char first = word[0];
        string rest = word.Length > 1
            ? ConvertTail(text, wordStart, word, 1, customMappings)
            : string.Empty;

        if (!char.IsUpper(first))
        {
            char? firstMapped = ConvertPlainChar(first, customMappings);
            return (firstMapped ?? first) + rest;
        }

        bool autoCapitalizedByWord = _autoCapAnalyzer.IsAutoCapitalizedByWord(text, wordStart, word);

        // Case 1: Probably user pressed Shift.
        // Use the Shift map directly.
        if (!autoCapitalizedByWord)
        {
            char? shiftedFirst = ConvertPlainChar(first, customMappings);
            if (shiftedFirst.HasValue)
                return shiftedFirst.Value + rest;

            char lower = char.ToLowerInvariant(first);
            return (ConvertPlainChar(lower, customMappings) ?? lower) + rest;
        }

        // Case 2: Word may have auto-capitalized the first letter.
        // Only accept Shift if the whole corrected word is known.
        char? autoShiftChar = ConvertPlainChar(first, customMappings);
        if (autoShiftChar.HasValue)
        {
            string candidate = autoShiftChar.Value + rest;

            if (corrections.Count > 0 && _wordMatcher.IsEquivalentWord(candidate, corrections))
                return candidate;
        }

        // Otherwise assume the uppercase letter was produced by Word auto-capitalization.
        char lowerFirst = char.ToLowerInvariant(first);
        return (ConvertPlainChar(lowerFirst, customMappings) ?? lowerFirst) + rest;
    }

    /// <summary>
    ///     Converts the tail of a word.
    ///     Mid-word uppercase letters are treated as Shift letters,
    ///     except standalone-like "I" that Word may have auto-capitalized.
    /// </summary>
    private string ConvertTail(
        string text,
        int wordStart,
        string word,
        int startIndex,
        IDictionary<char, char>? customMappings)
    {
        if (startIndex >= word.Length)
            return string.Empty;

        var sb = new StringBuilder(word.Length - startIndex);

        for (int i = startIndex; i < word.Length; i++)
        {
            char c = word[i];
            int absoluteIndex = wordStart + i;

            // Special case:
            // Word may turn standalone "i" into "I" after separators.
            if (c == 'I' && _autoCapAnalyzer.IsAutoLowercaseIAt(text, absoluteIndex))
            {
                sb.Append(ConvertPlainChar('i', customMappings) ?? 'i');
                continue;
            }

            char? mapped = ConvertNonInitialChar(c, customMappings);
            sb.Append(mapped ?? c);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Resolves a non-first character of a word.
    ///     Lowercase letters use the normal map.
    ///     Uppercase letters use the Shift map, then fallback to lowercase.
    /// </summary>
    private char? ConvertNonInitialChar(char c, IDictionary<char, char>? customMappings)
    {
        return ConvertPlainChar(c, customMappings);
    }

    /// <summary>
    ///     Returns whether a character belongs inside a word for conversion purposes.
    ///     Besides letters and digits, ZWNJ is kept as part of the word.
    /// </summary>
    private bool IsConversionWordChar(char c, IDictionary<char, char>? customMappings)
    {
        if (char.IsLetterOrDigit(c) || c == '\u200C')
            return true;

        char? mapped = ConvertPlainChar(c, customMappings);
        return mapped.HasValue && (char.IsLetter(mapped.Value) || char.IsDigit(mapped.Value));
    }

    /// <summary>
    ///     Resolves a plain character via custom mappings, then default lowercase map.
    /// </summary>
    private char? ConvertPlainChar(char c, IDictionary<char, char>? customMappings)
    {
        if (customMappings is not null && customMappings.TryGetValue(c, out var customMapped))
            return customMapped;

        if (MappingDefaults.GetDefaultEnglishToPersianMap().TryGetValue(c, out var mapped))
            return mapped;

        return null;
    }

    /// <summary>
    ///     Builds a normalized HashSet from word corrections.
    ///     Trimming is important because some default entries may have trailing spaces.
    /// </summary>
    private static HashSet<string> BuildCorrectionSet(IReadOnlyCollection<string>? words)
    {
        if (words is null || words.Count == 0)
            return EmptyCorrections;

        var set = new HashSet<string>(StringComparer.Ordinal);

        foreach (string? word in words)
        {
            if (string.IsNullOrWhiteSpace(word))
                continue;

            string trimmed = word.Trim();
            if (trimmed.Length > 0)
                set.Add(trimmed);
        }

        return set.Count == 0 ? EmptyCorrections : set;
    }
}