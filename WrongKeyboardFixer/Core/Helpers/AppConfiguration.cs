namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
/// Centralized application configuration constants.
/// All hardcoded values are defined here for easy maintenance and consistency.
/// </summary>
public static class AppConfiguration
{
    // ──────────────────────────────────────────────────────────────────────────
    // Clipboard Operations Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Clipboard
    {
        /// <summary>Maximum number of retry attempts when reading clipboard.</summary>
        public const int MaxRetryAttempts = 8;

        /// <summary>Delay in milliseconds between clipboard read retries.</summary>
        public const int RetryDelayMs = 150;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Text Conversion Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Conversion
    {
        /// <summary>Delay after sending Ctrl+C before reading clipboard (ms).</summary>
        public const int ClipboardReadDelayMs = 300;

        /// <summary>Delay after setting clipboard before sending Ctrl+V (ms).</summary>
        public const int ClipboardWriteDelayMs = 200;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Keyboard Simulator Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Keyboard
    {
        /// <summary>Delay after releasing modifier keys before sending combination (ms).</summary>
        public const int KeyReleaseDelayMs = 30;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Update Operations Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Update
    {
        /// <summary>Buffer size for downloading update assets (64 KiB).</summary>
        public const int DownloadBufferSize = 65536;

        /// <summary>Buffer size for copying files during extraction (8 KiB).</summary>
        public const int CopyBufferSize = 8192;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Text Direction Detection Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class TextDetection
    {
        /// <summary>Threshold ratio for Persian character detection (35%).</summary>
        public const double PersianThresholdRatio = 0.35;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Persian Word Matching Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class WordMatching
    {
        /// <summary>Maximum depth for word reduction (suffix removal).</summary>
        public const int MaxDepth = 4;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // File and Path Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Paths
    {
        /// <summary>Application settings folder name.</summary>
        public const string SettingsFolder = "WrongKeyboardFixer";

        /// <summary>Application settings file name.</summary>
        public const string SettingsFileName = "settings.json";
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Registry Configuration
    // ──────────────────────────────────────────────────────────────────────────
    public static class Registry
    {
        /// <summary>Windows Registry path for auto-start programs.</summary>
        public const string AutoStartRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>Registry value name for the application.</summary>
        public const string AutoStartValueName = "WrongKeyboardFixer";
    }
}