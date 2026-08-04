using System;
using System.Collections.Generic;

namespace WrongKeyboardFixer;

/// <summary>
/// Central localization manager for the application.
/// Supports Persian (fa), English (en).
/// This is a custom implementation for Native AOT compatibility
/// (standard .resx satellite assemblies don't work well with AOT trimming).
/// </summary>
public static class Localization
{
    public static class Languages
    {
        public const string Persian = "fa";
        public const string English = "en";
    }

    private static readonly Dictionary<string, string> Fa = new()
    {
        // General
        ["Error"] = "خطا",
        ["Warning"] = "اخطار",
        ["Attention"] = "توجه",
        ["Success"] = "موفقیت",
        ["Update"] = "بروزرسانی",

        // Settings form
        ["Settings"] = "تنظیمات",
        ["SettingsTitle"] = "تنظیمات برنامه",
        ["Language"] = "زبان:",
        ["LanguagePersian"] = "فارسی",
        ["LanguageEnglish"] = "English",
        ["LanguageFrench"] = "Français",
        ["RunOnStartup"] = "اجرای خودکار با ویندوز",
        ["HotkeyGroup"] = "تنظیمات کلید میانبر",
        ["HotkeyLabel"] = "کلید ترکیبی:",
        ["ApplyHotkey"] = "اعمال کلید ترکیبی",
        ["StatusChecking"] = "وضعیت: بررسی...",
        ["StatusRegistered"] = "✅ کلید ترکیبی اعمال شده است",
        ["StatusNotRegistered"] = "⚠️ کلید ترکیبی اعمال نشده است",
        ["StatusRegisterSuccess"] = "✅ کلید ترکیبی با موفقیت ثبت شده است",
        ["StatusRegisterFailed"] = "❌ ثبت ناموفق بود. کلید قبلاً ثبت شده است.",
        ["HotkeyRegisterSuccess"] = "کلید ترکیبی جدید با موفقیت ثبت شد.",
        ["CurrentVersion"] = "نسخه فعلی:",
        ["CheckUpdate"] = "بررسی بروزرسانی",
        ["Save"] = "ذخیره",
        ["Cancel"] = "انصراف",
        ["SettingsSaved"] = "تنظیمات با موفقیت ذخیره شد.",

        // Status messages
        ["CheckingUpdate"] = "بررسی بروزرسانی",
        ["CheckingProgress"] = "در حال بررسی...",

        // Tray menu
        ["TraySettings"] = "تنظیمات",
        ["TrayCheckUpdate"] = "بررسی بروزرسانی",
        ["TrayExit"] = "خروج",
        ["TrayText"] = "Wrong Keyboard Fixer",

        // Main form errors
        ["StartupError"] = "خطا در راه‌اندازی برنامه: {0}",
        ["HotkeyRegisterFailed"] = "ثبت Hotkey ناموفق بود. ممکن است کلید ترکیبی توسط برنامه دیگری گرفته شده باشد.",

        // Settings manager errors
        ["SaveSettingsError"] = "خطا در ذخیره تنظیمات: {0}",
        ["StartupSettingsError"] = "خطا در تنظیم اجرای خودکار: {0}",

        // Program
        ["AlreadyRunning"] = "برنامه از قبل در حال اجراست!",

        // AutoUpdater
        ["UpdCheckingCurrent"] = "بررسی نسخه فعلی...",
        ["UpdFetchingRelease"] = "دریافت اطلاعات آخرین نسخه از GitHub...",
        ["UpdFetchError"] = "خطا در دریافت اطلاعات بروزرسانی از سرور.\nلطفاً اتصال اینترنت خود را بررسی کنید.",
        ["UpdNoUpdate"] = "شما از آخرین نسخه استفاده می‌کنید.\n\nنسخه فعلی: {0}",
        ["UpdNoUpdateShort"] = "شما از آخرین نسخه استفاده می‌کنید",
        ["UpdFound"] = "نسخه جدید {0} یافت شد",
        ["UpdAvailablePrompt"] = "نسخه جدید {0} منتشر شده است.\nنسخه فعلی: {1}\n\nآیا می‌خواهید آن را دانلود و نصب کنید؟",
        ["UpdAvailableTitle"] = "بروزرسانی موجود است",
        ["UpdCheckError"] = "خطا در بررسی بروزرسانی:\n{0}",
        ["UpdCheckErrorInternet"] = "خطا در بررسی بروزرسانی. لطفاً اتصال اینترنت خود را بررسی کنید.",
        ["UpdDownloading"] = "در حال دانلود بروزرسانی...",
        ["UpdDownloadPercent"] = "دانلود: {0}%",
        ["UpdPreparing"] = "در حال آماده‌سازی نصب...",
        ["UpdExeNotFound"] = "فایل اجرایی در فایل فشرده پیدا نشد.",
        ["UpdNotNewer"] = "نسخه فایل دانلودشده ({0}) از نسخه فعلی ({1}) جدیدتر نیست.\nلطفاً مطمئن شوید که فایل ZIP حاوی نسخه جدیدتر است.",
        ["UpdInstalling"] = "در حال نصب... برنامه مجدداً اجرا می‌شود",
        ["UpdInstallComplete"] = "نصب کامل شد. برنامه مجدداً اجرا می‌شود.",
        ["UpdDownloadInstallError"] = "خطا در دانلود یا نصب بروزرسانی:\n{0}",
    };

    private static readonly Dictionary<string, string> En = new()
    {
        // General
        ["Error"] = "Error",
        ["Warning"] = "Warning",
        ["Attention"] = "Attention",
        ["Success"] = "Success",
        ["Update"] = "Update",

        // Settings form
        ["Settings"] = "Settings",
        ["SettingsTitle"] = "Application Settings",
        ["Language"] = "Language:",
        ["LanguagePersian"] = "فارسی",
        ["LanguageEnglish"] = "English",
        ["LanguageFrench"] = "Français",
        ["RunOnStartup"] = "Run on Windows startup",
        ["HotkeyGroup"] = "Hotkey Settings",
        ["HotkeyLabel"] = "Hotkey:",
        ["ApplyHotkey"] = "Apply Hotkey",
        ["StatusChecking"] = "Status: Checking...",
        ["StatusRegistered"] = "✅ Hotkey is applied",
        ["StatusNotRegistered"] = "⚠️ Hotkey is not applied",
        ["StatusRegisterSuccess"] = "✅ Hotkey registered successfully",
        ["StatusRegisterFailed"] = "❌ Registration failed. The hotkey is already taken.",
        ["HotkeyRegisterSuccess"] = "New hotkey registered successfully.",
        ["CurrentVersion"] = "Current version:",
        ["CheckUpdate"] = "Check for Updates",
        ["Save"] = "Save",
        ["Cancel"] = "Cancel",
        ["SettingsSaved"] = "Settings saved successfully.",

        // Status messages
        ["CheckingUpdate"] = "Checking for Updates",
        ["CheckingProgress"] = "Checking...",

        // Tray menu
        ["TraySettings"] = "Settings",
        ["TrayCheckUpdate"] = "Check for Updates",
        ["TrayExit"] = "Exit",
        ["TrayText"] = "Wrong Keyboard Fixer",

        // Main form errors
        ["StartupError"] = "Error starting the application: {0}",
        ["HotkeyRegisterFailed"] = "Hotkey registration failed. The key combination may be taken by another application.",

        // Settings manager errors
        ["SaveSettingsError"] = "Error saving settings: {0}",
        ["StartupSettingsError"] = "Error setting up auto-start: {0}",

        // Program
        ["AlreadyRunning"] = "The application is already running!",

        // AutoUpdater
        ["UpdCheckingCurrent"] = "Checking current version...",
        ["UpdFetchingRelease"] = "Fetching latest release info from GitHub...",
        ["UpdFetchError"] = "Error fetching update information from the server.\nPlease check your internet connection.",
        ["UpdNoUpdate"] = "You are using the latest version.\n\nCurrent version: {0}",
        ["UpdNoUpdateShort"] = "You are using the latest version",
        ["UpdFound"] = "New version {0} found",
        ["UpdAvailablePrompt"] = "New version {0} is available.\nCurrent version: {1}\n\nDo you want to download and install it?",
        ["UpdAvailableTitle"] = "Update Available",
        ["UpdCheckError"] = "Error checking for updates:\n{0}",
        ["UpdCheckErrorInternet"] = "Error checking for updates. Please check your internet connection.",
        ["UpdDownloading"] = "Downloading update...",
        ["UpdDownloadPercent"] = "Downloading: {0}%",
        ["UpdPreparing"] = "Preparing installation...",
        ["UpdExeNotFound"] = "Executable file not found in the archive.",
        ["UpdNotNewer"] = "The downloaded file version ({0}) is not newer than the current version ({1}).\nPlease make sure the ZIP file contains a newer version.",
        ["UpdInstalling"] = "Installing... The application will restart",
        ["UpdInstallComplete"] = "Installation complete. The application will restart.",
        ["UpdDownloadInstallError"] = "Error downloading or installing update:\n{0}",
    };

    private static Dictionary<string, string> _current = En;

    public static event EventHandler? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = Languages.English;

    public static bool IsRtl => CurrentLanguage == Languages.Persian;

    /// <summary>
    /// Sets the current language and raises the LanguageChanged event if it changed.
    /// </summary>
    public static void SetLanguage(string language)
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
    /// Returns the localized string for the given key.
    /// Falls back to the key itself if no translation is found.
    /// </summary>
    public static string Get(string key)
    {
        if (_current.TryGetValue(key, out var value))
            return value;
        return key;
    }

    /// <summary>
    /// Formats a localized string with the given arguments.
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
}