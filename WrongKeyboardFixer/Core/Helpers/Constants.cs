namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
///     Centralized constants for the application.
///     All hardcoded values have been moved here for easy maintenance.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     UI constants for controls
    /// </summary>
    public static class UI
    {
        public const int CornerRadius = 7;
        public const int ComboBoxItemHeight = 30;
        public const int ComboBoxDropDownHeight = 240;
        public const int ButtonPadding = 0;
        public const int CornerRadiusSmall = 6;
        public const int ArrowZoneWidth = 24;
        public const int PaddingSmall = 8;
        public const int ChevronOffset = 15;
        public const int ChevronHeight = 4;
        public const int ButtonWidth = 30;
        public const int ButtonHeight = 30;
    }

    /// <summary>
    ///     Windows messages
    /// </summary>
    public static class WindowsMessages
    {
        public const int WM_HOTKEY = 0x0312;
        public const int WM_SETREDRAW = 0x000B;
    }
}