using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Helpers;

public static partial class Localization
{
    /// <summary>Builds the English (en) dictionary.</summary>
    private static partial Dictionary<string, string> BuildEnDictionary()
    {
        return new Dictionary<string, string>
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

            // Word corrections
            ["WordCorrections"] = "Word Corrections (آ / ژ)",
            ["WordCorrectionsHint"] = "Words that must begin with «آ» or «ژ»; corrected if converted wrongly",
            ["WordCount"] = "Words: {0}",
             ["AddWord"] = "Add Word",
             ["RemoveWord"] = "Remove Word",
             ["Search"] = "Search",
            ["ResetWords"] = "Reset",
            ["EnterWord"] = "New word:",
            ["InvalidWord"] = "Please enter a valid word (must start with «آ» or «ژ»).",
             ["WordAlreadyExists"] = "This word is already in the list.",
             ["WordContainsSpace"] = "The word cannot contain spaces.",
             ["WordContainsNonBreakingSpace"] = "The word cannot contain non-breaking space (نیم فاصله).",
             ["ResetWordsConfirm"] = "Reset the word list to defaults?",

            // Clipboard
            ["ClipboardSetTextError"] = "Failed to set clipboard text",
        };
    }
}
