using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

/// <summary>
/// Central design system for the application: colors, fonts and control factories.
/// Modern Flat Light theme.
/// </summary>
public static class Theme
{
    // ── Brand ──────────────────────────────────────────────
    public static readonly Color Accent = Color.FromArgb(59, 105, 245);      // #3B69F5
    public static readonly Color AccentDark = Color.FromArgb(46, 85, 210);
    public static readonly Color AccentSoft = Color.FromArgb(238, 243, 255);
    public static readonly Color AccentBorder = Color.FromArgb(180, 199, 250);

    // ── Surfaces ───────────────────────────────────────────
    public static readonly Color Background = Color.FromArgb(244, 246, 250);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 249, 252);
    public static readonly Color SurfaceMuted = Color.FromArgb(243, 244, 248);

    // ── Borders ────────────────────────────────────────────
    public static readonly Color Border = Color.FromArgb(229, 233, 240);
    public static readonly Color BorderStrong = Color.FromArgb(214, 219, 230);

    // ── Text ───────────────────────────────────────────────
    public static readonly Color TextPrimary = Color.FromArgb(24, 32, 46);
    public static readonly Color TextSecondary = Color.FromArgb(106, 115, 133);
    public static readonly Color TextDisabled = Color.FromArgb(163, 170, 184);
    public static readonly Color TextOnAccent = Color.White;

    // ── Status ─────────────────────────────────────────────
    public static readonly Color Success = Color.FromArgb(34, 173, 92);
    public static readonly Color SuccessSoft = Color.FromArgb(232, 249, 238);
    public static readonly Color Warning = Color.FromArgb(222, 138, 18);
    public static readonly Color WarningSoft = Color.FromArgb(255, 248, 230);
    public static readonly Color Danger = Color.FromArgb(230, 76, 60);
    public static readonly Color DangerSoft = Color.FromArgb(254, 236, 233);
    public static readonly Color Info = Color.FromArgb(47, 128, 237);
    public static readonly Color InfoSoft = Color.FromArgb(232, 241, 253);

    // ── Typography ─────────────────────────────────────────
    // ── فونت‌ها کمی بزرگ‌تر برای خوانایی فارسی ──
    public static readonly Font TitleFont = new("Segoe UI", 13f, FontStyle.Bold);
    public static readonly Font SubtitleFont = new("Segoe UI", 9.5f);
    public static readonly Font SectionFont = new("Segoe UI", 10.5f, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 10f);
    public static readonly Font BodyBoldFont = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font SmallFont = new("Segoe UI", 9f);
    public static readonly Font ButtonFont = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font GridFont = new("Segoe UI", 10f);

    // ── DPI scaling (design baseline: 96 DPI) ──────────────
    public const int DesignDpi = 96;

    /// <summary>
    /// Returns the DPI scale factor relative to the 96 DPI design baseline.
    /// </summary>
    public static float DpiScale(int deviceDpi) => deviceDpi / (float)DesignDpi;

    /// <summary>
    /// Scales a design-time (96 DPI) pixel value to the target DPI.
    /// </summary>
    public static int DpiScale(int value, int deviceDpi) => (int)MathF.Round(value * DpiScale(deviceDpi));

    /// <summary>
    /// Scales a design-time (96 DPI) float pixel value to the target DPI.
    /// </summary>
    public static float DpiScaleF(float value, int deviceDpi) => value * DpiScale(deviceDpi);

    /// <summary>
    /// Creates a rounded-rectangle GraphicsPath.
    /// </summary>
    public static GraphicsPath RoundRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (radius <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        int d = radius * 2;
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>
    /// Detects whether a string contains RTL text (Arabic/Persian/Hebrew).
    /// </summary>
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

    /// <summary>
    /// Creates a centered-format StringFormat that respects the text direction.
    /// </summary>
    public static StringFormat CreateCenterFormat(string text)
    {
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        if (IsRtlText(text))
            format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
        return format;
    }

    // ── Factories ──────────────────────────────────────────

    public static Label SectionLabel(string text) => new()
    {
        Text = text,
        Font = SectionFont,
        ForeColor = Accent,
        AutoSize = true,              // قبلاً false بود با عرض پیش‌فرض 100px → متن بریده می‌شد
        TextAlign = ContentAlignment.MiddleLeft
    };

    public static Label BodyLabel(string text, Color? fore = null, Font? font = null) => new()
    {
        Text = text,
        Font = font ?? BodyFont,
        ForeColor = fore ?? TextPrimary,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleLeft
    };

    public static ComboBox CreateCombo(params string[] items)
    {
        var combo = new ModernComboBox();
        combo.Items.AddRange(items);
        return combo;
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.BackgroundColor = Surface;
        grid.GridColor = Color.FromArgb(238, 241, 248);
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;  // حذف حاشیهٔ سه‌بعدی Raised
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToOrderColumns = false;
        grid.AllowUserToResizeRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ScrollBars = ScrollBars.Vertical;

        int dpi = grid.DeviceDpi;
        int cellPad = DpiScale(6, dpi);
        grid.RowTemplate.Height = DpiScale(38, dpi);
        grid.ColumnHeadersHeight = DpiScale(40, dpi);
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            Font = GridFont,
            BackColor = Surface,
            ForeColor = TextPrimary,
            SelectionBackColor = AccentSoft,
            SelectionForeColor = TextPrimary,
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(cellPad, 0, cellPad, 0)
        };
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(250, 251, 253)
        };
        int headerPad = DpiScale(4, dpi);
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            Font = BodyBoldFont,
            BackColor = SurfaceAlt,
            ForeColor = TextSecondary,
            SelectionBackColor = SurfaceAlt,
            SelectionForeColor = TextSecondary,
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Padding = new Padding(headerPad, 0, headerPad, 0)
        };
    }

    public static void WarmUpFonts()
    {
        const string sample = "تنظیم نگاشت کیبورد برنامه ABC abc 123 +";
        foreach (var f in new[] { TitleFont, SubtitleFont, SectionFont, BodyFont,
                     BodyBoldFont, SmallFont, ButtonFont, GridFont })
            TextRenderer.MeasureText(sample, f);
    }

    /// <summary>
    /// A modern flat ContextMenuStrip renderer for the tray menu.
    /// </summary>
    public static ToolStripRenderer CreateMenuRenderer() =>
        new ToolStripProfessionalRenderer(new ModernMenuColorTable());

    private sealed class ModernMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Surface;
        public override Color MenuBorder => Border;
        public override Color MenuItemBorder => AccentSoft;
        public override Color MenuItemSelected => AccentSoft;
        public override Color MenuItemSelectedGradientBegin => AccentSoft;
        public override Color MenuItemSelectedGradientEnd => AccentSoft;
        public override Color MenuItemPressedGradientBegin => SurfaceMuted;
        public override Color MenuItemPressedGradientEnd => SurfaceMuted;
        public override Color ImageMarginGradientBegin => SurfaceAlt;
        public override Color ImageMarginGradientMiddle => SurfaceAlt;
        public override Color ImageMarginGradientEnd => SurfaceAlt;
        public override Color SeparatorDark => Border;
        public override Color SeparatorLight => Surface;
    }
}

/// <summary>
/// Small fluent helper for initializers that return a value (e.g. AddRange).
/// </summary>
internal static class ControlExtensions
{
    public static T Then<T>(this T value, Action<T> action)
    {
        action(value);
        return value;
    }
}
