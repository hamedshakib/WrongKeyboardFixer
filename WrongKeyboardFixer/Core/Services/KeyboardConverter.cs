using System;
using System.Collections.Generic;
using System.Text;
using WrongKeyboardFixer.Core.Contracts;
using WrongKeyboardFixer.Core.Model;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Converts characters between Persian and English based on user-defined mappings.
/// Supports bidirectional conversion for clipboard text.
/// </summary>
public sealed class KeyboardConverter : ITextConverter
{
    // Default maps are built once and reused (avoid rebuilding them for every character).
    private static readonly IReadOnlyDictionary<char, char> DefaultEnglishToPersianMap =
        MappingDefaults.GetDefaultEnglishToPersianMap();
    
    private static readonly IReadOnlyDictionary<char, char> DefaultMiddlePositionEnglishToPersianMap =
        MappingDefaults.GetDefaultMiddlePositionEnglishToPersianMap();

    /// <summary>
    /// Convert a single Persian character to English character using custom mappings.
    /// </summary>
    public char? ConvertPersianToEnglish(char persianChar, IReadOnlyDictionary<char, char>? customMappings = null)
    {
        // Check custom mapping first
        if (customMappings != null && customMappings.TryGetValue(persianChar, out var englishChar))
        {
            return englishChar;
        }

        // No default mapping - returns null if not found
        return null;
    }

    /// <summary>
    /// Convert a single English character to Persian character using custom mappings.
    /// </summary>
    public char? ConvertEnglishToPersian(char englishChar, IReadOnlyDictionary<char, char>? customMappings = null)
    {
        // Check custom mapping first
        if (customMappings != null && customMappings.TryGetValue(englishChar, out var persianChar))
        {
            return persianChar;
        }

        // No default mapping - returns null if not found
        return null;
    }

    /// <summary>
    /// Convert text from Persian to English.
    /// </summary>
    public string ConvertPersianToEnglish(string text, IReadOnlyDictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text)) return text;

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
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert text from English to Persian.
    /// </summary>
    public string ConvertEnglishToPersian(
        string text,
        IReadOnlyDictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // Determine if the character is at the start of a word
            bool isWordStart =
                i == 0 ||
                !char.IsLetterOrDigit(text[i - 1]);

            char? mapped = ConvertEnglishToPersian(
                c,
                customMappings,
                isWordStart);

            if (mapped.HasValue)
            {
                sb.Append(mapped.Value);
                continue;
            }

            // For characters without direct mapping, try lowercase
            char lowerChar = char.ToLowerInvariant(c);

            mapped = ConvertEnglishToPersian(
                lowerChar,
                customMappings,
                isWordStart);

            if (mapped.HasValue)
            {
                sb.Append(mapped.Value);
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    private char? ConvertEnglishToPersian(
        char c,
        IReadOnlyDictionary<char, char>? customMappings,
        bool isWordStart)
    {
        // 1. User custom mapping always has priority
        if (customMappings != null &&
            customMappings.TryGetValue(c, out var customMapped))
        {
            return customMapped;
        }

        // 2. If uppercase English letter and in middle of word, check special mapping
        if (char.IsUpper(c) && !isWordStart &&
            DefaultMiddlePositionEnglishToPersianMap.TryGetValue(c, out var middleMapped))
        {
            return middleMapped;
        }

        // 3. Normal mapping
        if (DefaultEnglishToPersianMap.TryGetValue(c, out var mapped))
        {
            return mapped;
        }

        return null;
    }

    /// <summary>
    /// Detect if text is mostly English or Persian to determine conversion direction.
    /// </summary>
    public bool ShouldConvertToPersian(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        int persianCount = 0;
        foreach (char c in text)
        {
            if (c >= 0x0600 && c <= 0x06FF) persianCount++;
        }

        // If less than 35% Persian, assume English input that needs conversion to Persian
        return persianCount < text.Length * 0.35;
    }
}