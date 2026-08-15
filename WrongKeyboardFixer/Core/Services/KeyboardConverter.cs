using System;
using System.Collections.Generic;
using System.Linq;
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
    /// </summary>
    public static char? ConvertEnglishToPersian(
        char englishChar,
        IDictionary<char, char>? customMappings = null)
    {
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

        bool autoCapitalizedByWord = TextAnalyzer.IsAutoCapitalizedByWord(text, wordStart, word);

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

            if (corrections.Count > 0 && PersianWordMatcher.IsEquivalentWord(candidate, corrections))
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
            if (c == 'I' && TextAnalyzer.IsAutoLowercaseIAt(text, absoluteIndex))
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
}

public static class PersianWordMatcher
{
    private static readonly string[] EncliticPronouns =
    {
        "شان",
        "تان",
        "مان",
        "ش",
        "ت",
        "م"
    };

    private static readonly string[] PluralSuffixes =
    {
        "ها",
        "ان"
    };

    /// <summary>
    /// بررسی می‌کند که candidate خودش یا یکی از اشکال صرفی/ترکیبی آن
    /// قبلاً در corrections وجود داشته است یا خیر.
    /// </summary>
    public static bool IsEquivalentWord(
        string candidate,
        IReadOnlySet<string> corrections)
    {
        if (string.IsNullOrWhiteSpace(candidate) || corrections.Count == 0)
            return false;

        candidate = Normalize(candidate);

        // 1. تطبیق مستقیم
        if (corrections.Contains(candidate))
            return true;

        // 2. نیم‌فاصله / فاصله
        //
        // مثلا:
        // کتاب‌مان
        // کتاب مان
        //
        // هر بخش به صورت مستقل بررسی می‌شود.
        foreach (var part in SplitParts(candidate))
        {
            if (part.Length == 0)
                continue;

            if (IsEquivalentSingleWord(part, corrections))
                return true;
        }

        // 3. خود candidate را نیز به صورت یک کلمه بررسی کنیم.
        return IsEquivalentSingleWord(candidate, corrections);
    }

    private static bool IsEquivalentSingleWord(
        string word,
        IReadOnlySet<string> corrections)
    {
        if (corrections.Contains(word))
            return true;

        // برای جلوگیری از loop
        var visited = new HashSet<string>(StringComparer.Ordinal);

        return TryReduce(
            word,
            corrections,
            visited,
            maxDepth: 4);
    }

    private static bool TryReduce(
        string word,
        IReadOnlySet<string> corrections,
        HashSet<string> visited,
        int maxDepth)
    {
        if (corrections.Contains(word))
            return true;

        if (maxDepth <= 0)
            return false;

        if (!visited.Add(word))
            return false;

        foreach (var reduced in GenerateReductions(word))
        {
            if (reduced.Length == 0)
                continue;

            if (corrections.Contains(reduced))
                return true;

            if (TryReduce(
                    reduced,
                    corrections,
                    visited,
                    maxDepth - 1))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// تمام حالت‌هایی که می‌توان از word با حذف وابسته‌های مجاز
    /// به دست آورد.
    /// </summary>
    private static IEnumerable<string> GenerateReductions(string word)
    {
        var results = new HashSet<string>(StringComparer.Ordinal);

        // ------------------------------------------------------------
        // 1. ضمایر پی‌بستی
        //
        // قلبم    -> قلب
        // قلبت    -> قلب
        // قلبش    -> قلب
        // قلبمان  -> قلب
        // قلبتان  -> قلب
        // قلبشان  -> قلب
        // ------------------------------------------------------------
        foreach (var suffix in EncliticPronouns)
        {
            if (!word.EndsWith(
                    suffix,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var baseWord = word[..^suffix.Length];

            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // ------------------------------------------------------------
        // 2. جمع "ها"
        //
        // کتاب‌ها -> کتاب
        // کتابها  -> کتاب
        // ------------------------------------------------------------
        if (word.EndsWith("ها", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];

            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // ------------------------------------------------------------
        // 3. جمع "ان"
        //
        // مثلا:
        // دوستان -> دوست
        //
        // توجه: این Rule عمداً محدود است و فقط نتیجه‌ای را معتبر
        // می‌دانیم که در corrections وجود داشته باشد.
        // ------------------------------------------------------------
        if (word.EndsWith("ان", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];

            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // ------------------------------------------------------------
        // 4. "یی"
        //
        // آرزویی -> آرزو
        //
        // آرزو + یی
        // ------------------------------------------------------------
        if (word.EndsWith("یی", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];

            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // ------------------------------------------------------------
        // 5. "ی"
        //
        // بعضی ساخت‌ها:
        // کتابی -> کتاب
        //
        // ولی نتیجه فقط زمانی قبول می‌شود که کتاب واقعاً در dictionary
        // وجود داشته باشد؛ بنابراین "علی" به صورت خودکار "عل" قبول نمی‌شود.
        // ------------------------------------------------------------
        if (word.EndsWith("ی", StringComparison.Ordinal))
        {
            var baseWord = word[..^1];

            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // ------------------------------------------------------------
        // 6. حالت "ها + ضمیر"
        //
        // کتاب‌هایمان
        //
        // ابتدا:
        // کتاب‌هایمان -> کتاب‌ها
        //
        // و سپس در recursion:
        // کتاب‌ها -> کتاب
        // ------------------------------------------------------------
        foreach (var pronoun in EncliticPronouns)
        {
            var pluralWithPronoun = "ها" + pronoun;

            if (word.EndsWith(
                    pluralWithPronoun,
                    StringComparison.Ordinal))
            {
                var baseWord = word[..^pluralWithPronoun.Length];

                if (baseWord.Length > 1)
                    results.Add(baseWord);

                // همچنین یک مرحله فقط ضمیر را حذف کنیم:
                // کتاب‌هایمان -> کتاب‌ها
                var withoutPronoun =
                    word[..^pronoun.Length];

                if (withoutPronoun.Length > 1)
                    results.Add(withoutPronoun);
            }
        }

        // ------------------------------------------------------------
        // 7. "ی + ضمیر"
        //
        // مثلا در برخی ساخت‌ها:
        // ...
        // ------------------------------------------------------------
        foreach (var pronoun in EncliticPronouns)
        {
            var suffix = "ی" + pronoun;

            if (!word.EndsWith(
                    suffix,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var baseWord = word[..^suffix.Length];

            if (baseWord.Length > 1)
                results.Add(baseWord);

            var withoutPronoun =
                word[..^pronoun.Length];

            if (withoutPronoun.Length > 1)
                results.Add(withoutPronoun);
        }

        return results;
    }

    /// <summary>
    /// Normalize کردن Unicode و فاصله‌ها.
    /// </summary>
    private static string Normalize(string value)
    {
        value = value.Trim();

        // ي -> ی
        value = value.Replace('ي', 'ی');

        // ك -> ک
        value = value.Replace('ك', 'ک');

        // ZWNJ
        value = value.Replace('\u200C', '\u200C');

        return value;
    }

    /// <summary>
    /// کلمه را روی فاصله و نیم‌فاصله می‌شکند.
    /// </summary>
    private static IEnumerable<string> SplitParts(string value)
    {
        return value
            .Split(
                new[]
                {
                    ' ',
                    '\t',
                    '\r',
                    '\n',
                    '\u200C' // Zero Width Non-Joiner
                },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize);
    }
}
