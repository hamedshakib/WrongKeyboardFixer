using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Helpers;

public static partial class Localization
{
    /// <summary>Builds the Persian (fa) dictionary.</summary>
    private static partial Dictionary<string, string> BuildFaDictionary()
    {
        return new Dictionary<string, string>
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

            // Word corrections
            ["WordCorrections"] = "کلمات تصحیح (آ / ژ)",
            ["WordCorrectionsHint"] = "کلماتی که باید با «آ» یا «ژ» شروع شوند؛ اگر هنگام تبدیل اشتباه شدند اصلاح می‌شوند",
            ["WordCount"] = "کلمات: {0}",
            ["AddWord"] = "افزودن کلمه",
            ["RemoveWord"] = "حذف کلمه",
            ["ResetWords"] = "بازنشانی",
            ["EnterWord"] = "کلمهٔ جدید:",
            ["InvalidWord"] = "لطفاً یک کلمهٔ معتبر وارد کنید (باید با «آ» یا «ژ» شروع شود).",
            ["WordAlreadyExists"] = "این کلمه از قبل در لیست وجود دارد.",
            ["ResetWordsConfirm"] = "آیا مطمئن هستید که می‌خواهید لیست کلمات را به پیش‌فرض برگردانید؟",

            // Clipboard
            ["ClipboardSetTextError"] = "Failed to set clipboard text",
        };
    }
}
