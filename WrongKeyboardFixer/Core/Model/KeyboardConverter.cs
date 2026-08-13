using System.Collections.Generic;
using System.Text;

namespace WrongKeyboardFixer.Core.Model;

/// <summary>
/// Converts characters between Persian and English based on user-defined mappings.
/// Supports bidirectional conversion for clipboard text.
/// </summary>
public static class KeyboardConverter
{
    // Default maps are built once and reused (avoid rebuilding them for every character).
    private static readonly IReadOnlyDictionary<char, char> DefaultEnglishToPersianMap =
        MappingDefaults.GetDefaultEnglishToPersianMap();

    private static readonly IReadOnlyDictionary<char, char> DefaultMiddlePositionEnglishToPersianMap =
        MappingDefaults.GetDefaultMiddlePositionEnglishToPersianMap();

    // If less than this fraction of the text is Persian, assume English input
    // that needs conversion to Persian.
    private const double PersianThresholdRatio = 0.35;

    /// <summary>
    /// Convert Persian character to English character using custom mappings.
    /// </summary>
    public static char? ConvertPersianToEnglish(char persianChar, IDictionary<char, char>? customMappings = null)
    {
        if (customMappings is not null && customMappings.TryGetValue(persianChar, out var englishChar))
            return englishChar;
        return null;
    }

    /// <summary>
    /// Convert English character to Persian character using custom mappings.
    /// </summary>
    public static char? ConvertEnglishToPersian(char englishChar, IDictionary<char, char>? customMappings = null)
    {
        if (customMappings is not null && customMappings.TryGetValue(englishChar, out var persianChar))
            return persianChar;
        return null;
    }

    /// <summary>
    /// Convert text from Persian to English.
    /// </summary>
    public static string ConvertPersianToEnglish(string? text, IDictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            char? mapped = ConvertPersianToEnglish(c, customMappings);
            if (mapped.HasValue)
            {
                char result = mapped.Value;
                if (char.IsUpper(c))
                    result = char.ToUpperInvariant(result);
                sb.Append(result);
            }
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Convert text from English to Persian.
    /// </summary>
    public static string ConvertEnglishToPersian(string? text, IDictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var sb = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool isWordStart = i == 0 || !char.IsLetterOrDigit(text[i - 1]);

            char? mapped = ConvertEnglishToPersianChar(c, customMappings, isWordStart);
            if (mapped.HasValue)
            {
                sb.Append(mapped.Value);
                continue;
            }

            // Only uppercase letters have a lowercase equivalent in the maps;
            // skipping this lookup for digits/punctuation/Persian chars avoids
            // a redundant dictionary probe per character.
            if (char.IsUpper(c))
            {
                char lowerChar = char.ToLowerInvariant(c);
                mapped = ConvertEnglishToPersianChar(lowerChar, customMappings, isWordStart);
                if (mapped.HasValue)
                {
                    sb.Append(mapped.Value);
                    continue;
                }
            }

            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Resolves a single English character to its Persian equivalent,
    /// consulting user mappings, the middle-of-word uppercase map,
    /// and finally the default map.
    /// </summary>
    private static char? ConvertEnglishToPersianChar(
        char c,
        IDictionary<char, char>? customMappings,
        bool isWordStart)
    {
        // 1. User customizations always take priority
        if (customMappings is not null &&
            customMappings.TryGetValue(c, out var customMapped))
            return customMapped;

        // 2. If the character is an uppercase English letter in the middle
        //    of a word, consult the middle-position uppercase map.
        if (char.IsUpper(c) && !isWordStart &&
            DefaultMiddlePositionEnglishToPersianMap.TryGetValue(c, out var middleMapped))
            return middleMapped;

        // 3. Standard lowercase/default map
        if (DefaultEnglishToPersianMap.TryGetValue(c, out var mapped))
            return mapped;

        return null;
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
}
