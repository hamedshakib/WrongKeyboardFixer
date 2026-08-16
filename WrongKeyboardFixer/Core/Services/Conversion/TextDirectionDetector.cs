using System;

namespace WrongKeyboardFixer.Core.Services.Conversion;

/// <summary>
/// Analyzes text to determine if it should be converted to Persian or English.
/// Uses Unicode block detection to identify Persian/Arabic characters.
/// </summary>
public sealed class TextDirectionDetector
{
    private const double DefaultPersianThresholdRatio = 0.35;

    /// <summary>
    /// Analyzes the text and determines if it should be converted to Persian.
    /// Returns true if the text is mostly English (should convert to Persian),
    /// or false if it's mostly Persian (should convert to English).
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <param name="persianThresholdRatio">The threshold ratio for Persian character detection.</param>
    /// <returns>True if text should be converted TO Persian, false otherwise.</returns>
    public bool ShouldConvertToPersian(string? text, double? persianThresholdRatio = null)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        var threshold = persianThresholdRatio ?? DefaultPersianThresholdRatio;
        int persianCount = 0;

        foreach (char c in text)
        {
            // Arabic / Persian Unicode block: U+0600 - U+06FF
            if (c >= 0x0600 && c <= 0x06FF)
                persianCount++;
        }

        return persianCount < text.Length * threshold;
    }
}