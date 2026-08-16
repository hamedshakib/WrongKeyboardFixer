using System;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
/// Analyzes text to detect Word auto-capitalization patterns.
/// Detects positions where Word may have auto-capitalized letters or
/// auto-lowercased standalone "I".
/// </summary>
public sealed class WordAutoCapitalizationAnalyzer
{
    /// <summary>
    /// Determines whether an uppercase first letter may have been created by Word auto-capitalization.
    /// </summary>
    /// <param name="text">The full text.</param>
    /// <param name="wordStart">The starting index of the word in the text.</param>
    /// <param name="word">The word to analyze.</param>
    /// <returns>True if the word was likely auto-capitalized by Word.</returns>
    public bool IsAutoCapitalizedByWord(string text, int wordStart, string word)
    {
        if (string.IsNullOrEmpty(word))
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
    /// <param name="text">The full text.</param>
    /// <param name="index">The index of the character to check.</param>
    /// <returns>True if the character at index is a standalone "I" that was auto-lowercased.</returns>
    public bool IsAutoLowercaseIAt(string text, int index)
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

    /// <summary>
    /// Detects positions where Word often auto-capitalizes the next word:
    /// - start of text
    /// - after newline / paragraph
    /// - after tab / table cell separator
    /// - after sentence terminators: . ? !
    /// - after bullets or numbered list markers
    /// </summary>
    /// <param name="text">The full text.</param>
    /// <param name="start">The starting index of the word.</param>
    /// <returns>True if the position is likely a Word auto-capitalize position.</returns>
    private bool IsAtWordAutoCapitalizePosition(string text, int start)
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

    private bool IsStandaloneWordChar(char c) =>
        char.IsLetterOrDigit(c) || c == '_' || c == '\u200C';

    private bool IsNewLine(char c) =>
        c == '\n' ||
        c == '\r' ||
        c == '\u0085' ||
        c == '\u2028' ||
        c == '\u2029';

    private bool IsSentenceTerminator(char c) =>
        c == '.' ||
        c == '!' ||
        c == '?' ||
        c == '\u061F'; // Persian question mark "?"

    private bool IsListMarker(string text, int index)
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

    private bool IsBulletChar(char c) =>
        c == '-' ||
        c == '*' ||
        c == '+' ||
        c == '\u2022' || //bullet
        c == '\u00B7' || //middle dot
        c == '\u25E6' || //white bullet
        c == '\u2023' || //hyphen bullet
        c == '\u25AA' || //black small square
        c == '\u25CB' || //white circle
        c == '\u25CF' || //black circle
        c == '\u203A';   //right-pointing single quotation mark

    private bool IsAtLineStartIgnoringSpaces(string text, int index)
    {
        int i = index - 1;

        // Skip non-newline whitespace only.
        while (i >= 0 && char.IsWhiteSpace(text[i]) && !IsNewLine(text[i]))
            i--;

        if (i < 0)
            return true;

        return IsNewLine(text[i]);
    }

    private bool HasNumberOrLetterListMarkerBefore(string text, int delimiterIndex)
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