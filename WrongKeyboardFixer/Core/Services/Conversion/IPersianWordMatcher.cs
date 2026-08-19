using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
///     Defines the interface for Persian word matching and normalization.
/// </summary>
public interface IPersianWordMatcher
{
    /// <summary>
    ///     Checks if the candidate word (or any of its inflected/combined forms)
    ///     exists in the corrections set.
    /// </summary>
    /// <param name="candidate">The word to check.</param>
    /// <param name="corrections">The set of known correct words.</param>
    /// <returns>True if the word or any of its forms is in corrections.</returns>
    bool IsEquivalentWord(string candidate, IReadOnlySet<string> corrections);
}