using System;
using System.Collections.Generic;
using System.Text;
using WrongKeyboardFixer.Core.Models;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Converts characters between Persian and English based on user-defined mappings.
/// The English-to-Persian conversion includes a heuristic for Word-like auto-capitalization.
/// </summary>
public static class KeyboardConverter
{
    private static readonly IReadOnlyDictionary<char, char> DefaultEnglishToPersianMap =
        MappingDefaults.GetDefaultEnglishToPersianMap();

    private static readonly IReadOnlyDictionary<char, char> DefaultEnglishToPersianShiftMap =
        MappingDefaults.GetDefaultEnglishToPersianShiftMap();

    private static readonly IReadOnlyDictionary<char, char> DefaultPersianToEnglishMap =
        MappingDefaults.GetDefaultPersianToEnglishMap();

    private static readonly HashSet<string> EmptyCorrections =
        new HashSet<string>(StringComparer.Ordinal);

    private static readonly HashSet<string> DefaultWordCorrections =
        BuildCorrectionSet(MappingDefaults.GetDefaultWordCorrections());

    private const double PersianThresholdRatio = 0.35;

    /// <summary>
    /// Convert Persian character to English character using custom mappings, then defaults.
    /// </summary>
    public static char? ConvertPersianToEnglish(
        char persianChar,
        IDictionary<char, char>? customMappings = null)
    {
        if (customMappings is not null && customMappings.TryGetValue(persianChar, out var englishChar))
            return englishChar;

        if (DefaultPersianToEnglishMap.TryGetValue(persianChar, out var defaultEnglishChar))
            return defaultEnglishChar;

        return null;
    }

    /// <summary>
    /// Convert English character to Persian character.
    /// For a single character without context, uppercase characters are treated as Shift characters.
    /// </summary>
    public static char? ConvertEnglishToPersian(
        char englishChar,
        IDictionary<char, char>? customMappings = null)
    {
        if (char.IsUpper(englishChar))
        {
            var shifted = ConvertShiftChar(englishChar, customMappings);
            if (shifted.HasValue)
                return shifted.Value;

            return ConvertPlainChar(char.ToLowerInvariant(englishChar), customMappings);
        }

        return ConvertPlainChar(englishChar, customMappings);
    }

    /// <summary>
    /// Convert text from Persian to English.
    /// </summary>
    public static string ConvertPersianToEnglish(
        string? text,
        IDictionary<char, char>? customMappings = null)
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
    /// Convert text from English to Persian.
    ///
    /// Uppercase first letters are resolved with a Word auto-capitalization heuristic:
    /// - If the word is in a position where Word may have auto-capitalized it,
    ///   the Shift mapping is accepted only when the converted word is in <paramref name="wordCorrections"/>.
    /// - Otherwise, the uppercase letter is assumed to be intentional Shift.
    ///
    /// If <paramref name="wordCorrections"/> is null, the cached default correction list is used.
    /// </summary>
    public static string ConvertEnglishToPersian(
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
    /// Detect if text is mostly English or Persian to determine conversion direction.
    /// </summary>
    public static bool ShouldConvertToPersian(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        int persianCount = 0;

        foreach (char c in text)
        {
            // Arabic / Persian Unicode block: U+0600 – U+06FF
            if (c >= 0x0600 && c <= 0x06FF)
                persianCount++;
        }

        return persianCount < text.Length * PersianThresholdRatio;
    }

    /// <summary>
    /// Converts one English word with attention to Word auto-capitalization context.
    /// </summary>
    private static string ConvertWordWithContext(
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

        bool autoCapitalizedByWord = IsAutoCapitalizedByWord(text, wordStart, word);

        // Case 1: Probably user pressed Shift.
        // Use the Shift map directly.
        if (!autoCapitalizedByWord)
        {
            char? shiftedFirst = ConvertShiftChar(first, customMappings);
            if (shiftedFirst.HasValue)
                return shiftedFirst.Value + rest;

            char lower = char.ToLowerInvariant(first);
            return (ConvertPlainChar(lower, customMappings) ?? lower) + rest;
        }

        // Case 2: Word may have auto-capitalized the first letter.
        // Only accept Shift if the whole corrected word is known.
        char? autoShiftChar = ConvertShiftChar(first, customMappings);
        if (autoShiftChar.HasValue)
        {
            string candidate = autoShiftChar.Value + rest;

            if (corrections.Count > 0 && corrections.Contains(candidate))
                return candidate;
        }

        // Otherwise assume the uppercase letter was produced by Word auto-capitalization.
        char lowerFirst = char.ToLowerInvariant(first);
        return (ConvertPlainChar(lowerFirst, customMappings) ?? lowerFirst) + rest;
    }

    /// <summary>
    /// Converts the tail of a word.
    /// Mid-word uppercase letters are treated as Shift letters,
    /// except standalone-like "I" that Word may have auto-capitalized.
    /// </summary>
    private static string ConvertTail(
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
            // Example: ";I" should become "که", not "ک]".
            if (c == 'I' && IsAutoLowercaseIAt(text, absoluteIndex))
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
    /// Resolves a non-first character of a word.
    /// Lowercase letters use the normal map.
    /// Uppercase letters use the Shift map, then fallback to lowercase.
    /// </summary>
    private static char? ConvertNonInitialChar(
        char c,
        IDictionary<char, char>? customMappings)
    {
        if (char.IsUpper(c))
        {
            char? shifted = ConvertShiftChar(c, customMappings);
            if (shifted.HasValue)
                return shifted.Value;

            return ConvertPlainChar(char.ToLowerInvariant(c), customMappings);
        }

        return ConvertPlainChar(c, customMappings);
    }

    /// <summary>
    /// Returns whether <paramref name="c"/> belongs inside a word for conversion purposes.
    /// Besides letters and digits, ZWNJ is kept as part of the word, and so is any
    /// character that converts to a Persian letter or digit.
    /// Example: ',' maps to «و», so mistyped words containing ',' stay together.
    /// </summary>
    private static bool IsConversionWordChar(
        char c,
        IDictionary<char, char>? customMappings)
    {
        if (char.IsLetterOrDigit(c) || c == '\u200C')
            return true;

        char? mapped = ConvertPlainChar(c, customMappings);
        return mapped.HasValue && (char.IsLetter(mapped.Value) || char.IsDigit(mapped.Value));
    }

    /// <summary>
    /// Resolves a plain character via custom mappings, then default lowercase map.
    /// </summary>
    private static char? ConvertPlainChar(
        char c,
        IDictionary<char, char>? customMappings)
    {
        if (customMappings is not null && customMappings.TryGetValue(c, out var customMapped))
            return customMapped;

        if (DefaultEnglishToPersianMap.TryGetValue(c, out var mapped))
            return mapped;

        return null;
    }

    /// <summary>
    /// Resolves a Shift character via custom mappings, then default Shift map.
    /// </summary>
    private static char? ConvertShiftChar(
        char c,
        IDictionary<char, char>? customMappings)
    {
        if (customMappings is not null && customMappings.TryGetValue(c, out var customMapped))
            return customMapped;

        if (DefaultEnglishToPersianShiftMap.TryGetValue(c, out var mapped))
            return mapped;

        return null;
    }

    /// <summary>
    /// Builds a normalized HashSet from word corrections.
    /// Trimming is important because some default entries may have trailing spaces.
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

    /// <summary>
    /// Determines whether an uppercase first letter may have been created by Word auto-capitalization.
    /// </summary>
    private static bool IsAutoCapitalizedByWord(
        string text,
        int wordStart,
        string word)
    {
        if (word.Length == 0)
            return false;

        if (!char.IsUpper(word[0]))
            return false;

        // Special case: standalone "I" is often Word correcting lowercase "i".
        if (IsAutoLowercaseIAt(text, wordStart))
            return true;

        return IsAtWordAutoCapitalizePosition(text, wordStart);
    }

    /// <summary>
    /// Detects Word's standalone "i" to "I" correction at any position.
    /// Examples:
    /// "I"      -> auto lowercase i
    /// ";I"     -> auto lowercase i
    /// "(I)"    -> auto lowercase i
    /// "I,"     -> auto lowercase i
    /// "In"     -> not auto lowercase i, because I is followed by a letter
    /// </summary>
    private static bool IsAutoLowercaseIAt(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return false;

        if (text[index] != 'I')
            return false;

        // If previous character is a word character, it is not standalone.
        if (index > 0 && IsStandaloneWordChar(text[index - 1]))
            return false;

        // If next character is a word character, it is not standalone.
        if (index + 1 < text.Length && IsStandaloneWordChar(text[index + 1]))
            return false;

        return true;
    }

    

    private static bool IsStandaloneWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '\u200C';
    }

    /// <summary>
    /// Detects positions where Word often auto-capitalizes the next word:
    /// - start of text
    /// - after newline / paragraph
    /// - after tab / table cell separator
    /// - after sentence terminators: . ? !
    /// - after bullets or numbered list markers
    /// </summary>
    private static bool IsAtWordAutoCapitalizePosition(string text, int start)
    {
        if (start <= 0)
            return true;

        int i = start - 1;
        bool sawNewLine = false;
        bool sawTab = false;

        while (i >= 0 && char.IsWhiteSpace(text[i]))
        {
            char ch = text[i];

            if (IsNewLine(ch))
                sawNewLine = true;
            else if (ch == '\t')
                sawTab = true;

            i--;
        }

        // Start of text, possibly after spaces/newlines.
        if (i < 0)
            return true;

        // New paragraph / new line / new table row.
        if (sawNewLine)
            return true;

        // Plain-text table cell separator.
        if (sawTab)
            return true;

        char previous = text[i];

        // End of sentence.
        if (IsSentenceTerminator(previous))
            return true;

        // Bullet or numbered list marker.
        if (IsListMarker(text, i))
            return true;

        return false;
    }

    private static bool IsNewLine(char c)
    {
        return c == '\n' ||
               c == '\r' ||
               c == '\u0085' ||
               c == '\u2028' ||
               c == '\u2029';
    }

    private static bool IsSentenceTerminator(char c)
    {
        return c == '.' ||
               c == '!' ||
               c == '?' ||
               c == '\u061F'; // Persian question mark «؟»
    }

    private static bool IsListMarker(string text, int index)
    {
        if (index < 0)
            return false;

        char c = text[index];

        // Bullets: -, *, +, •, ·, ◦, ‣, ▪, ○, ●, ›
        if (IsBulletChar(c))
            return IsAtLineStartIgnoringSpaces(text, index);

        // Numbered/lettered list like "1)" or "a)".
        if (c == ')')
            return HasNumberOrLetterListMarkerBefore(text, index);

        // Note: "1." is already covered because '.' is treated as a sentence terminator.
        return false;
    }

    private static bool IsBulletChar(char c)
    {
        return c == '-' ||
               c == '*' ||
               c == '+' ||
               c == '\u2022' || // •
               c == '\u00B7' || // ·
               c == '\u25E6' || // ◦
               c == '\u2023' || // ‣
               c == '\u25AA' || // ▪
               c == '\u25CB' || // ○
               c == '\u25CF' || // ●
               c == '\u203A';   // ›
    }

    private static bool IsAtLineStartIgnoringSpaces(string text, int index)
    {
        int i = index - 1;

        // Skip non-newline whitespace only.
        while (i >= 0 && char.IsWhiteSpace(text[i]) && !IsNewLine(text[i]))
            i--;

        if (i < 0)
            return true;

        return IsNewLine(text[i]);
    }

    private static bool HasNumberOrLetterListMarkerBefore(string text, int delimiterIndex)
    {
        int i = delimiterIndex - 1;
        int markerLength = 0;

        while (i >= 0 && char.IsLetterOrDigit(text[i]))
        {
            i--;
            markerLength++;
        }

        if (markerLength == 0)
            return false;

        int markerStartIndex = i + 1;
        return IsAtLineStartIgnoringSpaces(text, markerStartIndex);
    }
}