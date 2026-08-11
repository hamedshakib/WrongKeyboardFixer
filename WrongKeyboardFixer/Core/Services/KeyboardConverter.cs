using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WrongKeyboardFixer.Core.Services;

/// <summary>
/// Converts characters between Persian and English based on user-defined mappings.
/// Supports bidirectional conversion for clipboard text.
/// </summary>
public static class KeyboardConverter
{
    /// <summary>
    /// Convert Persian character to English character using custom mappings.
    /// </summary>
    public static char? ConvertPersianToEnglish(char persianChar, Dictionary<char, char>? customMappings = null)
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
    /// Convert English character to Persian character using custom mappings.
    /// </summary>
    public static char? ConvertEnglishToPersian(char englishChar, Dictionary<char, char>? customMappings = null)
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
    public static string ConvertPersianToEnglish(string text, Dictionary<char, char>? customMappings = null)
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
    public static string ConvertEnglishToPersian(
        string text,
        Dictionary<char, char>? customMappings = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var sb = new StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // مشخص می‌کند حرف در ابتدای کلمه است یا خیر
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

            // برای حروفی که mapping مستقیم ندارند،
            // lowercase آن‌ها را نیز امتحان می‌کنیم.
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

    private static char? ConvertEnglishToPersian(
        char c,
        Dictionary<char, char>? customMappings,
        bool isWordStart)
    {
        // 1. Mapping اختصاصی کاربر همیشه اولویت دارد
        if (customMappings != null &&
            customMappings.TryGetValue(c, out var customMapped))
        {
            return customMapped;
        }

        // 2. اگر حرف بزرگ انگلیسی است و وسط کلمه قرار دارد،
        // mapping مخصوص آن را بررسی کن.
        if (char.IsUpper(c) && !isWordStart)
        {
            if (Core.Model.MappingDefaults.GetDefaultMiddlePositionEnglishToPersianMap().TryGetValue(c, out var middleMapped))
            {
                return middleMapped;
            }
        }

        // 3. Mapping عادی
        if (Core.Model.MappingDefaults.GetDefaultEnglishToPersianMap().TryGetValue(c, out var mapped))
        {
            return mapped;
        }

        return null;
    }

    /// <summary>
    /// Detect if text is mostly English or Persian to determine conversion direction.
    /// </summary>
    public static bool ShouldConvertToPersian(string text)
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