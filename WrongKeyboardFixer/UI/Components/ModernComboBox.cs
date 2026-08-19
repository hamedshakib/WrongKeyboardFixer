using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     A flat combo box with a rounded border, custom chevron and soft
///     selection highlight that matches the Modern Flat theme.
/// </summary>
public class ModernComboBox : ComboBox
{
    private bool _hovered;

    public ModernComboBox()
    {
        FlatStyle = FlatStyle.Flat;
        DropDownStyle = ComboBoxStyle.DropDownList;
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = Theme.DpiScale(Constants.UI.ComboBoxItemHeight, DeviceDpi);
        DropDownHeight = Theme.DpiScale(Constants.UI.ComboBoxDropDownHeight, DeviceDpi);
        IntegralHeight = false;
        Font = Theme.BodyFont;
        BackColor = Theme.Surface;
        ForeColor = Theme.TextPrimary;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        Invalidate();
        base.OnEnabledChanged(e);
    }

    protected override void ScaleCore(float dx, float dy)
    {
        base.ScaleCore(dx, dy);
        ItemHeight = Math.Max(1, (int)MathF.Round(ItemHeight * dy));
        DropDownHeight = Math.Max(1, (int)MathF.Round(DropDownHeight * dy));
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            base.OnDrawItem(e);
            return;
        }

        bool isEditArea = (e.State & DrawItemState.ComboBoxEdit) != 0;
        bool selected = (e.State & DrawItemState.Selected) != 0 && !isEditArea;
        bool rtl = RightToLeft == RightToLeft.Yes;
        int dpi = DeviceDpi;

        // ── پس‌زمینه (با پشتیبانی حالت غیرفعال) ──
        Color bg = selected ? Theme.AccentSoft
            : isEditArea && !Enabled ? Theme.SurfaceMuted
            : Theme.Surface;
        using (var b = new SolidBrush(bg))
        {
            e.Graphics.FillRectangle(b, e.Bounds);
        }

        string text = Items[e.Index]?.ToString() ?? "";

        // ── مستطیل متن: در قسمت نمایش، فضای فلش در سمت صحیح کم می‌شود ──
        int arrowZone = Theme.DpiScale(Constants.UI.ArrowZoneWidth, dpi);
        Rectangle textRect;
        if (isEditArea)
        {
            textRect = rtl
                ? new Rectangle(arrowZone, e.Bounds.Y, e.Bounds.Width - arrowZone - 2, e.Bounds.Height)
                : new Rectangle(2, e.Bounds.Y, e.Bounds.Width - arrowZone - 2, e.Bounds.Height);
        }
        else
        {
            int pad = Theme.DpiScale(8, dpi);
            textRect = new Rectangle(e.Bounds.X + pad, e.Bounds.Y, e.Bounds.Width - pad * 2, e.Bounds.Height);
        }

        // ── متن شارپ با GDI به‌جای DrawString تار ──
        var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
        if (rtl) flags |= TextFormatFlags.RightToLeft;

        Color fore = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
        TextRenderer.DrawText(e.Graphics, text, Font, textRect, fore, flags);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg != 0x000F) // WM_PAINT
            return;

        using var g = CreateGraphics();
        g.SmoothingMode = SmoothingMode.AntiAlias;

        bool isRtl = RightToLeft == RightToLeft.Yes;
        int dpi = DeviceDpi;

        // ── پوشاندن دکمهٔ پیش‌فرض سیستم‌عامل (با رنگ حالت غیرفعال) ──
        int coverWidth = Theme.DpiScale(Constants.UI.ButtonWidth, dpi);
        var coverRect = isRtl
            ? new Rectangle(0, 0, coverWidth, Height)
            : new Rectangle(Width - coverWidth, 0, coverWidth, Height);
        using (var bgBrush = new SolidBrush(Enabled ? Theme.Surface : Theme.SurfaceMuted))
        {
            g.FillRectangle(bgBrush, coverRect);
        }

        // ── حاشیهٔ گرد ──
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = Theme.RoundRect(rect, Theme.DpiScale(Constants.UI.CornerRadiusSmall, dpi)))
        using (var pen = new Pen(!Enabled ? Theme.Border : _hovered ? Theme.Accent : Theme.BorderStrong))
        {
            g.DrawPath(pen, path);
        }

        // ── فلش (با قلم مقیاس‌شده با DPI و رنگ حالت غیرفعال) ──
        int cx = isRtl ? Theme.DpiScale(Constants.UI.ChevronOffset, dpi) : Width - Theme.DpiScale(Constants.UI.ChevronOffset, dpi);
        int cy = Height / 2;
        using var chevron = new Pen(
            !Enabled ? Theme.TextDisabled : _hovered ? Theme.Accent : Theme.TextSecondary,
            Theme.DpiScaleF(1.6f, dpi));
        chevron.LineJoin = LineJoin.Round;
        int ch = Theme.DpiScale(Constants.UI.ChevronHeight, dpi);
        g.DrawLines(chevron, new[]
        {
            new PointF(cx - ch, cy - Theme.DpiScale(2, dpi)),
            new PointF(cx, cy + Theme.DpiScale(2, dpi)),
            new PointF(cx + ch, cy - Theme.DpiScale(2, dpi))
        });
    }
}