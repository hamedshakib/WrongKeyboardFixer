namespace WrongKeyboardFixer.Core.Helpers;

/// <summary>
///     Centralized constants for the application.
///     All hardcoded values have been moved here for easy maintenance.
/// </summary>
public static class Constants
{
    /// <summary>
    ///     UI constants for forms
    /// </summary>
    public static class Forms
    {
        public const int SettingsFormWidth = 560;
        public const int SettingsFormHeight = 600;
        public const int KeyboardMappingsFormWidth = 920;
        public const int KeyboardMappingsFormHeight = 860;
        public const int UpdateProgressDialogWidth = 400;
        public const int UpdateProgressDialogHeight = 120;
    }

    /// <summary>
    ///     UI constants for controls
    /// </summary>
    public static class UI
    {
        public const int CornerRadius = 7;
        public const int CornerRadiusSmall = 6;
        public const int CornerRadiusMedium = 8;
        
        public const int ComboBoxItemHeight = 30;
        public const int ComboBoxDropDownHeight = 240;
        public const int ButtonPadding = 0;
        public const int ArrowZoneWidth = 24;
        public const int PaddingSmall = 8;
        public const int ChevronOffset = 15;
        public const int ChevronHeight = 4;
        public const int ButtonWidth = 30;
        
        public const int StdButtonHeight = 36;
        public const int StdChipHeight = 36;
        public const int StdTextBoxHeight = 36;
        
        public const int RowGapSmall = 10;
        public const int RowGapMedium = 14;
        public const int RowGapLarge = 22;
        
        public const int GridColumnSmall = 30;
        public const int GridColumnMedium = 20;
        public const int GridColumnLarge = 50;
        
        public const int SectionGap = 22;
        public const int CardGap = 16;

        // ── Card and Panel dimensions ─────────────────────────
        public const int CardPadding = 14;
        public const int CardCornerRadius = 12;
        public const int CardCornerRadiusLarge = 14;
        public const int PanelPadding = 24;
        
        // ── Button sizes ─────────────────────────────────────
        public const int ButtonHeight = 38;
        public const int ButtonWidthSmall = 112;
        public const int ButtonWidthMedium = 160;
        public const int ButtonWidthLarge = 170;
        
        // ── Label sizes ──────────────────────────────────────
        public const int LabelCountWidth = 130;
        public const int LabelCountHeight = 22;
        
        // ── Grid spacing ─────────────────────────────────────
        public const int GridYMapping = 94;
        public const int GridYWord = 102;
        public const int GridHeightMapping = 250;
        public const int GridHeightWord = 132;
        public const int RightElementWidth = 110;
        
        // ── Footer and divider ───────────────────────────────
        public const int FooterHeight = 42;
        public const int FooterDividerOffset = 16;
        public const int FooterButtonSpacing = 124;
        
        // ── Word corrections card ────────────────────────────
        public const int WordCorrectionsCardHeight = 310;
        public const int WordCorrectionsSearchY = 42;
        public const int WordCorrectionsHintY = 76;
        public const int WordCorrectionsActionY = 112;
        public const int WordCorrectionsActionHeight = 34;
        public const int WordCorrectionsTextBoxOffset = 2;
        public const int WordCorrectionsTextBoxHeight = 30;
        
        // ── Input Box ────────────────────────────────────────
        public const int MappingInputBoxWidth = 380;
        public const int MappingInputBoxHeight = 250;
        public const int MappingInputBoxLabelX = 24;
        public const int MappingInputBoxLabelY = 18;
        public const int MappingInputBoxTextBoxWidth = 316;
        public const int MappingInputBoxTextBoxHeight = 30;
        public const int MappingInputBoxFirstTextY = 44;
        public const int MappingInputBoxSecondLabelY = 92;
        public const int MappingInputBoxSecondTextY = 118;
        public const int MappingInputBoxButtonY = 166;
        public const int MappingInputBoxOkButtonX = 124;
        public const int MappingInputBoxCancelButtonX = 240;
        public const int MappingInputBoxButtonWidth = 100;
        
        // ── ModernTextBox ────────────────────────────────────
        public const int ModernTextBoxHeight = 36;
        public const int ModernTextBoxWidth = 220;
        public const int ModernTextBoxCornerRadius = 8;
        public const int ModernTextBoxPadding = 12;
        
        // ── ModernButton ─────────────────────────────────────
        public const int ModernButtonHeight = 36;
        public const int ModernButtonCornerRadius = 7;
        
        // ── DPI Scaled constants ─────────────────────────────
        public const int DpiKnobSizeOffset = 6;
        public const int DpiTrackPadding = 8;
        public const int DpiCellPadding = 6;
        public const int GridRowTemplateHeight = 38;
        public const int GridColumnHeadersHeight = 40;
        public const int GridCellPadding = 6;
        public const int GridHeaderPadding = 4;
        
        // ── ModernTitleBar ───────────────────────────────────
        public const int TitleBarHeight = 56;
        public const int ControlAreaWidth = 96;
        public const int TitleBarButtonWidth = 46;
        public const int TitleBarTextStart = 34;
        public const int TitleBarIconX = 16;
        public const int TitleBarIconSize = 10;
        
        // ── ToggleSwitch ─────────────────────────────────────
        public const int ToggleSwitchWidth = 44;
        public const int ToggleSwitchHeight = 24;
        public const int ToggleSwitchKnobSizeOffset = 6;
        public const int ToggleSwitchGap = 10;
        
        // ── StatusChip ───────────────────────────────────────
        public const int StatusChipHeight = 36;
        public const int StatusChipCornerRadius = 8;
        public const int StatusChipPadding = 12;
        public const int StatusChipMinWidth = 24;
        
        // ── RoundedPanel ─────────────────────────────────────
        public const int RoundedPanelDefaultCornerRadius = 10;
        public const int RoundedPanelDefaultPadding = 16;
    }

    /// <summary>
    ///     Windows messages
    /// </summary>
    public static class WindowsMessages
    {
        public const int WM_HOTKEY = 0x0312;
        public const int WM_SETREDRAW = 0x000B;
        public const int WM_PAINT = 0x000F;
        public const int WM_NCLBUTTONDOWN = 0x00A1;
        public const int HTCAPTION = 0x0002;
    }
}
