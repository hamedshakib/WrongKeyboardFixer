using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.Core.UI;

/// <summary>
/// A rounded status label ("chip") used in the settings form.
/// Paints itself with the status color stored in <see cref="Control.Tag"/>
/// and sizes itself to its content so it never stretches across its row.
/// </summary>
public class StatusChip : Label
{
    public const int StdHeight = 36;

    public StatusChip(string text, Color fore, Color back)
    {
        Font = Theme.BodyBoldFont;
        ForeColor = fore;
        Tag = back;
        Text = text;
        AutoSize = false;
        Height = StdHeight;
        TextAlign = ContentAlignment.MiddleLeft;
        Padding = new Padding(12, 0, 12, 0);
        BackColor = Theme.Surface;   // همیشه هم‌رنگ پنل → گوشه‌های مربعی نامرئی

        Paint += (_, e) => PaintChip(e.Graphics);
        UpdateSize();
    }

    private void PaintChip(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundRect(bounds, Theme.DpiScale(8, DeviceDpi));
        using var brush = new SolidBrush(Tag is Color c ? c : Theme.Surface);
        g.FillPath(brush, path);

        int pad = Theme.DpiScale(12, DeviceDpi);
        var textRect = new Rectangle(pad, 0, Width - pad * 2, Height);
        var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        if (Localization.IsRtl) flags |= TextFormatFlags.RightToLeft;
        TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, flags);
    }

    /// <summary>Resizes the chip to fit its current text.</summary>
    public void UpdateSize()
    {
        int textWidth = TextRenderer.MeasureText(Text, Font).Width;
        Width = textWidth + Theme.DpiScale(24, DeviceDpi);
    }
}