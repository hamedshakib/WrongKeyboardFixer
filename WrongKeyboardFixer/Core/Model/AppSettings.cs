using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.Model;

/// <summary>
/// Application settings with validation and immutable defaults.
/// </summary>
public sealed class AppSettings
{
    private string _language = Localization.Languages.English;
    private Dictionary<char, char>? _persianToEnglishMap;
    private Dictionary<char, char>? _englishToPersianMap;

    /// <summary>
    /// Gets or sets whether the application runs on system startup.
    /// </summary>
    public bool RunOnStartup { get; set; } = false;

    /// <summary>
    /// Gets or sets the application language (e.g., "en" or "fa").
    /// </summary>
    public string Language
    {
        get => _language;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Language cannot be null or empty.", nameof(value));

            var normalizedValue = value.ToLowerInvariant();
            if (normalizedValue != Localization.Languages.English && 
                normalizedValue != Localization.Languages.Persian)
                throw new ArgumentException($"Invalid language: {value}. Must be 'en' or 'fa'.", nameof(value));

            _language = normalizedValue;
        }
    }

    /// <summary>
    /// Gets or sets the hotkey modifier flags (Ctrl, Alt, Shift, etc.).
    /// </summary>
    public int HotkeyModifier { get; set; } = (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt);

    /// <summary>
    /// Gets or sets the hotkey key.
    /// </summary>
    public Keys HotkeyKey { get; set; } = Keys.Add;

    /// <summary>
    /// Gets or sets the Persian to English character mapping.
    /// Returns a copy to prevent external mutation of internal state.
    /// </summary>
    public Dictionary<char, char> PersianToEnglishMap
    {
        get => _persianToEnglishMap ??= MappingDefaults.GetDefaultPersianToEnglishMap();
        set => _persianToEnglishMap = value != null ? new Dictionary<char, char>(value) : null;
    }

    /// <summary>
    /// Gets or sets the English to Persian character mapping.
    /// Returns a copy to prevent external mutation of internal state.
    /// </summary>
    public Dictionary<char, char> EnglishToPersianMap
    {
        get => _englishToPersianMap ??= MappingDefaults.GetDefaultEnglishToPersianMap();
        set => _englishToPersianMap = value != null ? new Dictionary<char, char>(value) : null;
    }

    /// <summary>
    /// Creates a deep copy of this settings instance.
    /// </summary>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            RunOnStartup = RunOnStartup,
            Language = Language,
            HotkeyModifier = HotkeyModifier,
            HotkeyKey = HotkeyKey,
            PersianToEnglishMap = PersianToEnglishMap != null ? new Dictionary<char, char>(PersianToEnglishMap) : null,
            EnglishToPersianMap = EnglishToPersianMap != null ? new Dictionary<char, char>(EnglishToPersianMap) : null
        };
    }
}

/// <summary>
/// Modifier flags for hotkey registration.
/// </summary>
[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Windows = 0x0008
}