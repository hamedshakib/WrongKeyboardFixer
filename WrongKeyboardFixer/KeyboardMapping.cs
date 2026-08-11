using System;
using System.Collections.Generic;

namespace WrongKeyboardFixer;

/// <summary>
/// Represents a character-to-character mapping for Persian-English conversion.
/// PersianChar: the Persian character
/// EnglishChar: the corresponding English character
/// </summary>
public class KeyboardMapping : ICloneable
{
    /// <summary>
    /// The Persian character.
    /// </summary>
    public char? PersianChar { get; set; }

    /// <summary>
    /// The English character.
    /// </summary>
    public char? EnglishChar { get; set; }

    /// <summary>
    /// Creates a new KeyboardMapping instance.
    /// </summary>
    /// <param name="persianChar">The Persian character.</param>
    /// <param name="englishChar">The English character.</param>
    public KeyboardMapping(char? persianChar = null, char? englishChar = null)
    {
        PersianChar = persianChar;
        EnglishChar = englishChar;
    }

    /// <summary>
    /// Creates a clone of this mapping.
    /// </summary>
    public object Clone()
    {
        return new KeyboardMapping(PersianChar, EnglishChar);
    }

    /// <summary>
    /// Returns a string representation of this mapping.
    /// </summary>
    public override string ToString()
    {
        string persian = PersianChar.HasValue ? PersianChar.Value.ToString() : "";
        string english = EnglishChar.HasValue ? EnglishChar.Value.ToString() : "";
        return $"{persian} <-> {english}";
    }
}
