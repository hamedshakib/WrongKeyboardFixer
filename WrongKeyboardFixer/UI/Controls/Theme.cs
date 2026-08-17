namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// Central design system: colors, fonts, and control factories for MAUI.
/// Mirrors the WinForms Theme.cs for visual consistency.
/// </summary>
public static class Theme
{
    // ── Brand ──
    public static readonly Color Accent = Color.FromArgb("#3B69F5");
    public static readonly Color AccentDark = Color.FromArgb("#2E55D2");
    public static readonly Color AccentSoft = Color.FromArgb("#EEF3FF");
    public static readonly Color AccentBorder = Color.FromArgb("#B4C7FA");

    // ── Surfaces ──
    public static readonly Color Background = Color.FromArgb("#F4F6FA");
    public static readonly Color Surface = Colors.White;
    public static readonly Color SurfaceAlt = Color.FromArgb("#F8F9FC");
    public static readonly Color SurfaceMuted = Color.FromArgb("#F3F4F8");

    // ── Borders ──
    public static readonly Color Border = Color.FromArgb("#E5E9F0");
    public static readonly Color BorderStrong = Color.FromArgb("#D6DBE6");

    // ── Text ──
    public static readonly Color TextPrimary = Color.FromArgb("#18202E");
    public static readonly Color TextSecondary = Color.FromArgb("#6A7385");
    public static readonly Color TextDisabled = Color.FromArgb("#A3AAB8");
    public static readonly Color TextOnAccent = Colors.White;

    // ── Status ──
    public static readonly Color Success = Color.FromArgb("#22AD5C");
    public static readonly Color SuccessSoft = Color.FromArgb("#E8F9EE");
    public static readonly Color Warning = Color.FromArgb("#DE8A12");
    public static readonly Color WarningSoft = Color.FromArgb("#FFF8E6");
    public static readonly Color Danger = Color.FromArgb("#E64C3C");
    public static readonly Color DangerSoft = Color.FromArgb("#FEECE9");
    public static readonly Color Info = Color.FromArgb("#2F80ED");
    public static readonly Color InfoSoft = Color.FromArgb("#E8F1FD");

    // ── Typography (MAUI FontSizes) ──
    public const double TitleFontSize = 18;
    public const double SubtitleFontSize = 13;
    public const double SectionFontSize = 14.5;
    public const double BodyFontSize = 14;
    public const double SmallFontSize = 12.5;
    public const double ButtonFontSize = 14;

    // ── Spacing ──
    public const double StdHeight = 40;
    public const double RowGap = 14;
    public const double SectionGap = 22;
    public const double CardPadding = 24;
    public const double CornerRadius = 12;

    // ── Helpers ──

    public static bool IsRtlText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (char c in text)
        {
            if (c >= 0x0590 && c <= 0x08FF)
                return true;
        }
        return false;
    }

    public static FlowDirection GetFlowDirection() =>
        Core.Helpers.Localization.IsRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    // ── Control Factories ──

    public static Label CreateSectionLabel(string text) => new()
    {
        Text = text,
        FontSize = SectionFontSize,
        TextColor = Accent,
        FontAttributes = FontAttributes.Bold,
        Margin = new Thickness(0, 0, 0, 4),
    };

    public static Label CreateBodyLabel(string text, Color? textColor = null, double? fontSize = null) => new()
    {
        Text = text,
        FontSize = fontSize ?? BodyFontSize,
        TextColor = textColor ?? TextPrimary,
    };

    public static ModernButton CreatePrimaryButton(string text, double? width = null)
    {
        var btn = new ModernButton
        {
            Text = text,
            ButtonVariant = ModernButton.Variant.Primary,
        };
        if (width.HasValue) btn.WidthRequest = width.Value;
        return btn;
    }

    public static ModernButton CreateSecondaryButton(string text, double? width = null)
    {
        var btn = new ModernButton
        {
            Text = text,
            ButtonVariant = ModernButton.Variant.Secondary,
        };
        if (width.HasValue) btn.WidthRequest = width.Value;
        return btn;
    }

    public static ModernButton CreateGhostButton(string text, double? width = null)
    {
        var btn = new ModernButton
        {
            Text = text,
            ButtonVariant = ModernButton.Variant.Ghost,
        };
        if (width.HasValue) btn.WidthRequest = width.Value;
        return btn;
    }

    public static Frame CreateRoundedFrame(Color? bgColor = null, double cornerRadius = 12, double? borderWidth = null) => new()
    {
        BackgroundColor = bgColor ?? SurfaceAlt,
        CornerRadius = new CornerRadius(cornerRadius),
        BorderColor = Border,
        BorderWidth = borderWidth ?? 1,
        Padding = new Thickness(14),
        HasShadow = false,
    };

    public static Frame CreateCardFrame() => CreateRoundedFrame(SurfaceAlt, 12, 1);

    public static BoxView CreateDivider() => new()
    {
        HeightRequest = 1,
        BackgroundColor = Border,
        Margin = new Thickness(0, SectionGap, 0, 0),
    };
}
