using System;
using System.Collections.Generic;
using System.Linq;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
///     Provides functionality to match and normalize Persian words, including
///     handling of enclitic pronouns, plural suffixes, and other word variations.
///     Used to determine if a converted word matches a known correct word.
/// </summary>
public sealed class PersianWordMatcher : IPersianWordMatcher
{
    private const int MaxDepth = 4;

    private static readonly string[] EncliticPronouns =
    {
        "شان", "تان", "مان", "ش", "ت", "م"
    };

    private static readonly string[] PluralSuffixes =
    {
        "ها", "ان"
    };

    /// <summary>
    ///     Checks if the candidate word (or any of its inflected/combined forms)
    ///     exists in the corrections set.
    /// </summary>
    /// <param name="candidate">The word to check.</param>
    /// <param name="corrections">The set of known correct words.</param>
    /// <returns>True if the word or any of its forms is in corrections.</returns>
    public bool IsEquivalentWord(string candidate, IReadOnlySet<string> corrections)
    {
        if (string.IsNullOrWhiteSpace(candidate) || corrections.Count == 0)
            return false;

        candidate = Normalize(candidate);

        // 1. Direct match
        if (corrections.Contains(candidate))
            return true;

        // 2. Split on spaces and ZWNJ
        foreach (var part in SplitParts(candidate))
        {
            if (part.Length == 0)
                continue;

            if (IsEquivalentSingleWord(part, corrections))
                return true;
        }

        // 3. Check the full candidate as a single word
        return IsEquivalentSingleWord(candidate, corrections);
    }

    private bool IsEquivalentSingleWord(string word, IReadOnlySet<string> corrections)
    {
        if (corrections.Contains(word))
            return true;

        // Avoid infinite loops
        var visited = new HashSet<string>(StringComparer.Ordinal);

        return TryReduce(word, corrections, visited, MaxDepth);
    }

    private bool TryReduce(string word, IReadOnlySet<string> corrections, HashSet<string> visited, int maxDepth)
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

            if (TryReduce(reduced, corrections, visited, maxDepth - 1))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Generates all possible reduced forms by removing allowed suffixes.
    /// </summary>
    /// <returns>Collection of possible base word forms.</returns>
    private IEnumerable<string> GenerateReductions(string word)
    {
        var results = new HashSet<string>(StringComparer.Ordinal);

        // 1. Enclitic pronouns (persian possessive endings)
        foreach (var suffix in EncliticPronouns)
        {
            if (!word.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            var baseWord = word[..^suffix.Length];
            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // 2. Plural "ha" (persian plural suffix)
        if (word.EndsWith("ها", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];
            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // 3. Plural "an"
        if (word.EndsWith("ان", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];
            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // 4. "yee" ending
        if (word.EndsWith("یی", StringComparison.Ordinal))
        {
            var baseWord = word[..^2];
            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // 5. "y" ending (adjective/noun ending)
        if (word.EndsWith("ی", StringComparison.Ordinal))
        {
            var baseWord = word[..^1];
            if (baseWord.Length > 1)
                results.Add(baseWord);
        }

        // 6. "ha + pronoun" combinations
        foreach (var pronoun in EncliticPronouns)
        {
            var pluralWithPronoun = "ها" + pronoun;
            if (word.EndsWith(pluralWithPronoun, StringComparison.Ordinal))
            {
                var baseWord = word[..^pluralWithPronoun.Length];
                if (baseWord.Length > 1)
                    results.Add(baseWord);

                // Also try removing just the pronoun
                var withoutPronoun = word[..^pronoun.Length];
                if (withoutPronoun.Length > 1)
                    results.Add(withoutPronoun);
            }
        }

        // 7. "y + pronoun" combinations
        foreach (var pronoun in EncliticPronouns)
        {
            var suffix = "ی" + pronoun;
            if (!word.EndsWith(suffix, StringComparison.Ordinal))
                continue;

            var baseWord = word[..^suffix.Length];
            if (baseWord.Length > 1)
                results.Add(baseWord);

            var withoutPronoun = word[..^pronoun.Length];
            if (withoutPronoun.Length > 1)
                results.Add(withoutPronoun);
        }

        return results;
    }

    /// <summary>
    ///     Normalizes Unicode and spacing in a Persian string.
    ///     - Trims whitespace
    ///     - Replaces Arabic 'yeh' with Persian 'yeh'
    ///     - Replaces Arabic 'kaf' with Persian 'kaf'
    ///     - Normalizes ZWNJ (Zero Width Non-Joiner)
    /// </summary>
    private string Normalize(string value)
    {
        value = value.Trim();

        // Arabic yeh -> Persian yeh
        value = value.Replace('ي', 'ی');

        // Arabic kaf -> Persian kaf
        value = value.Replace('ك', 'ک');

        // Normalize ZWNJ (already correct)
        value = value.Replace('\u200C', '\u200C');

        return value;
    }

    /// <summary>
    ///     Splits a word on spaces, tabs, carriage returns, newlines, and ZWNJ.
    ///     Returns normalized parts.
    /// </summary>
    private IEnumerable<string> SplitParts(string value)
    {
        return value
            .Split(new[]
            {
                ' ', '\t', '\r', '\n', '\u200C'
            }, StringSplitOptions.RemoveEmptyEntries)
            .Select(Normalize);
    }
}