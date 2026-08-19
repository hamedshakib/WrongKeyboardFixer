using System;
using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
///     Central localization manager for the application. Supports Persian (fa) and
///     English (en). This is a custom implementation for Native AOT compatibility
///     (standard .resx satellite assemblies don't work well with AOT trimming).
///     The language dictionaries live in separate partial files:
///     <see cref="Localization.Fa" /> data is defined in Localization.Fa.cs and
///     <see cref="Localization.En" /> data in Localization.En.cs.
/// </summary>
public static partial class Localization
{
    private static readonly Dictionary<string, string> Fa = BuildFaDictionary();
    private static readonly Dictionary<string, string> En = BuildEnDictionary();
    private static Dictionary<string, string> _current = En;

    public static string CurrentLanguage { get; private set; } = Languages.English;

    public static bool IsRtl => CurrentLanguage == Languages.Persian;

    /// <summary>Raised after the active language changes.</summary>
    public static event EventHandler? LanguageChanged;

    /// <summary>
    ///     Sets the current language and raises <see cref="LanguageChanged" /> if it changed.
    /// </summary>
    public static void SetLanguage(string? language)
    {
        language = language?.ToLowerInvariant() ?? Languages.English;

        if (language != Languages.Persian && language != Languages.English)
            language = Languages.English;

        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        _current = language switch
        {
            Languages.Persian => Fa,
            _ => En
        };

        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    ///     Returns the localized string for the given key.
    ///     Falls back to the key itself if no translation is found.
    /// </summary>
    public static string Get(string key)
    {
        if (_current.TryGetValue(key, out var value))
            return value;
        return key;
    }

    /// <summary>
    ///     Formats a localized string with the given arguments.
    /// </summary>
    public static string Format(string key, params object[] args)
    {
        try
        {
            return string.Format(Get(key), args);
        }
        catch (FormatException)
        {
            return Get(key);
        }
    }

    // Implemented in Localization.Fa.cs / Localization.En.cs
    private static partial Dictionary<string, string> BuildFaDictionary();
    private static partial Dictionary<string, string> BuildEnDictionary();

    public static class Languages
    {
        public const string Persian = "fa";
        public const string English = "en";
    }
}