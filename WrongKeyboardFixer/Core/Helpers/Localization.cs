using System;
using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Helpers;

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
        ["SettingsSubtitle"] = "رفتار برنامه و کلید میانبر را مدیریت کنید",
        ["Language"] = "زبان:",
        ["LanguagePersian"] = "فارسی",
        ["LanguageEnglish"] = "English",
        ["RunOnStartup"] = "اجرای خودکار با ویندوز",
        ["HotkeyGroup"] = "تنظیمات کلید میانبر",
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

        // Keyboard mappings
        ["KeyboardMappings"] = "نگاشت کیبورد",
        ["KeyboardMappingsTitle"] = "تنظیم نگاشت کیبورد",
        ["KeyboardMappingsSubtitle"] = "متن اشتباه‌تایپ‌شده را با این کلیدها اصلاح کنید",
        ["MappingsCount"] = "تعداد نگاشت‌ها: {0}",
        ["AddNewMapping"] = "افزودن نگاشت جدید",
        ["EditMapping"] = "ویرایش نگاشت",
        ["SelectRowHint"] = "برای ویرایش، یک ردیف را انتخاب کنید",
        ["DirectionEnglishToPersian"] = "انگلیسی به فارسی",
        ["DirectionPersianToEnglish"] = "فارسی به انگلیسی",
        ["MappingsHint"] = "نکته: میتوانید با دوبار کلیک روی ردیف، آن را ویرایش کنید",
        ["EnglishToPersian"] = "فارسی → انگلیسی",
        ["PersianToEnglish"] = "انگلیسی → فارسی",
        ["Key"] = "کلید",
        ["KeyName"] = "نام دکمه",
        ["PersianChar"] = "کارکتر فارسی",
        ["EnglishChar"] = "کارکتر انگلیسی",
        ["Reset"] = "بازنشانی",
        ["ResetAll"] = "بازنشانی همه به پیش‌فرض",
        ["ResetAllConfirm"] = "آیا مطمئن هستید که می‌خواهید تمام نگاشت‌های سفارشی را حذف کنید؟",
        ["ResetKeyConfirm"] = "آیا مطمئن هستید که می‌خواهید نگاشت سفارشی این کلید را حذف کنید؟",
        ["ResetSuccess"] = "نگاشت با موفقیت بازنشانی شد.",

        // Add mapping
        ["AddMapping"] = "افزودن نگاشت",
        ["EnterPersianChar"] = "کارکتر فارسی را وارد کنید:",
        ["EnterEnglishChar"] = "کارکتر انگلیسی را وارد کنید:",
        ["InvalidPersianChar"] = "لطفاً یک کارکتر فارسی معتبر وارد کنید.",
        ["InvalidEnglishChar"] = "لطفاً یک کارکتر انگلیسی معتبر وارد کنید.",
        ["MappingAddedSuccess"] = "نگاشت با موفقیت اضافه شد.",
        ["OK"] = "تأیید",
        ["Delete"] = "حذف",
        ["DeleteMappingConfirm"] = "آیا مطمئن هستید که می‌خواهید این نگاشت را حذف کنید؟",

        // Hotkey settings
        ["HotkeyModifierCtrlAlt"] = "Ctrl + Alt",
        ["HotkeyModifierCtrlShift"] = "Ctrl + Shift",
        ["HotkeyModifierAltShift"] = "Alt + Shift",
        ["HotkeyModifierCtrl"] = "Ctrl",
        ["HotkeyModifierAlt"] = "Alt",
        ["HotkeyModifierShift"] = "Shift",
        ["HotkeyKeyAdd"] = "Add (+)",
        ["HotkeyKeySubtract"] = "Subtract (-)",
        ["HotkeyKeyMultiply"] = "Multiply (*)",
        ["HotkeyKeyF1"] = "F1",
        ["HotkeyKeyF2"] = "F2",
        ["HotkeyKeyF3"] = "F3",
        ["HotkeyKeyF4"] = "F4",
        ["HotkeyKeyF5"] = "F5",
        ["HotkeyKeyF6"] = "F6",
        ["HotkeyKeyF7"] = "F7",
        ["HotkeyKeyF8"] = "F8",
        ["HotkeyKeyF9"] = "F9",
        ["HotkeyKeyF10"] = "F10",
        ["HotkeyKeyF11"] = "F11",
        ["HotkeyKeyF12"] = "F12",
        ["HotkeyKeyInsert"] = "Insert",
        ["HotkeyKeyHome"] = "Home",
        ["HotkeyKeyPageUp"] = "PageUp",
        ["HotkeyKeyPageDown"] = "PageDown",
        ["HotkeyKeyEnd"] = "End",
        ["HotkeyKeyDelete"] = "Delete",
        ["HotkeyKeySpace"] = "Space",

        // Clipboard
        ["ClipboardSetTextError"] = "Failed to set clipboard text",
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
        ["SettingsSubtitle"] = "Manage application behavior and hotkey",
        ["Language"] = "Language:",
        ["LanguagePersian"] = "فارسی",
        ["LanguageEnglish"] = "English",
        ["RunOnStartup"] = "Run on Windows startup",
        ["HotkeyGroup"] = "Hotkey Settings",
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

        // Keyboard mappings
        ["KeyboardMappings"] = "Keyboard Mappings",
        ["KeyboardMappingsTitle"] = "Keyboard Mapping Settings",
        ["KeyboardMappingsSubtitle"] = "Fix mistyped text with these key mappings",
        ["MappingsCount"] = "Mappings: {0}",
        ["AddNewMapping"] = "Add New Mapping",
        ["EditMapping"] = "Edit Mapping",
        ["SelectRowHint"] = "Select a row to edit",
        ["DirectionEnglishToPersian"] = "English to Persian",
        ["DirectionPersianToEnglish"] = "Persian to English",
        ["MappingsHint"] = "Tip: double-click a row to edit it",
        ["EnglishToPersian"] = "English → Persian",
        ["PersianToEnglish"] = "Persian → English",
        ["Key"] = "Key",
        ["KeyName"] = "Key Name",
        ["PersianChar"] = "Persian Char",
        ["EnglishChar"] = "English Char",
        ["Reset"] = "Reset",
        ["ResetAll"] = "Reset All to Default",
        ["ResetAllConfirm"] = "Are you sure you want to reset all custom mappings?",
        ["ResetKeyConfirm"] = "Are you sure you want to reset the custom mapping for this key?",
        ["ResetSuccess"] = "Mapping reset successfully.",

        // Add mapping
        ["AddMapping"] = "Add Mapping",
        ["EnterPersianChar"] = "Enter Persian character:",
        ["EnterEnglishChar"] = "Enter English character:",
        ["InvalidPersianChar"] = "Please enter a valid Persian character.",
        ["InvalidEnglishChar"] = "Please enter a valid English character.",
        ["MappingAddedSuccess"] = "Mapping added successfully.",
        ["OK"] = "OK",
        ["Delete"] = "Delete",
        ["DeleteMappingConfirm"] = "Are you sure you want to delete this mapping?",

        // Hotkey settings
        ["HotkeyModifierCtrlAlt"] = "Ctrl + Alt",
        ["HotkeyModifierCtrlShift"] = "Ctrl + Shift",
        ["HotkeyModifierAltShift"] = "Alt + Shift",
        ["HotkeyModifierCtrl"] = "Ctrl",
        ["HotkeyModifierAlt"] = "Alt",
        ["HotkeyModifierShift"] = "Shift",
        ["HotkeyKeyAdd"] = "Add (+)",
        ["HotkeyKeySubtract"] = "Subtract (-)",
        ["HotkeyKeyMultiply"] = "Multiply (*)",
        ["HotkeyKeyF1"] = "F1",
        ["HotkeyKeyF2"] = "F2",
        ["HotkeyKeyF3"] = "F3",
        ["HotkeyKeyF4"] = "F4",
        ["HotkeyKeyF5"] = "F5",
        ["HotkeyKeyF6"] = "F6",
        ["HotkeyKeyF7"] = "F7",
        ["HotkeyKeyF8"] = "F8",
        ["HotkeyKeyF9"] = "F9",
        ["HotkeyKeyF10"] = "F10",
        ["HotkeyKeyF11"] = "F11",
        ["HotkeyKeyF12"] = "F12",
        ["HotkeyKeyInsert"] = "Insert",
        ["HotkeyKeyHome"] = "Home",
        ["HotkeyKeyPageUp"] = "PageUp",
        ["HotkeyKeyPageDown"] = "PageDown",
        ["HotkeyKeyEnd"] = "End",
        ["HotkeyKeyDelete"] = "Delete",
        ["HotkeyKeySpace"] = "Space",

        // Clipboard
        ["ClipboardSetTextError"] = "Failed to set clipboard text",
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
