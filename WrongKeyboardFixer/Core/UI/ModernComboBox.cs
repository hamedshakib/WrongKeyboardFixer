using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

/// <summary>
/// A flat combo box with a rounded border, custom chevron and soft
/// selection highlight that matches the Modern Flat theme.
/// </summary>
public class ModernComboBox : ComboBox
{
    private bool _hovered;

    public ModernComboBox()
    {
        FlatStyle = FlatStyle.Flat;
        DropDownStyle = ComboBoxStyle.DropDownList;
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = Theme.DpiScale(30, DeviceDpi);
        DropDownHeight = Theme.DpiScale(240, DeviceDpi);
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

        using (var bg = new SolidBrush(isEditArea ? Theme.Surface : selected ? Theme.AccentSoft : Theme.Surface))
            e.Graphics.FillRectangle(bg, e.Bounds);

        string text = Items[e.Index]?.ToString() ?? "";
        var format = Theme.CreateCenterFormat(text);
        format.Alignment = StringAlignment.Center;

        int padding = Theme.DpiScale(isEditArea ? 22 : 8, DeviceDpi);
        var textRect = new Rectangle(e.Bounds.X + Theme.DpiScale(2, DeviceDpi), e.Bounds.Y, e.Bounds.Width - padding - Theme.DpiScale(2, DeviceDpi), e.Bounds.Height);

        using (var textBrush = new SolidBrush(Theme.TextPrimary))
            e.Graphics.DrawString(text, Font, textBrush, textRect, format);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (m.Msg != 0x000F) // WM_PAINT
            return;

        using var g = CreateGraphics();

        // ---------------------------------------------------------
        // FIX: پشتیبانی از RTL برای پوشاندن دکمه پیش‌فرض سیستم‌عامل
        // ---------------------------------------------------------
        int coverWidth = Theme.DpiScale(30, DeviceDpi);
        bool isRtl = RightToLeft == RightToLeft.Yes;

        var coverRect = isRtl
            ? new Rectangle(0, 0, coverWidth, Height) // پوشاندن سمت چپ در حالت فارسی
            : new Rectangle(Width - coverWidth, 0, coverWidth, Height); // پوشاندن سمت راست در انگلیسی

        using (var bgBrush = new SolidBrush(Theme.Surface))
        {
            g.FillRectangle(bgBrush, coverRect);
        }
        // ---------------------------------------------------------

        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundRect(rect, Theme.DpiScale(6, DeviceDpi));
        using (var pen = new Pen(_hovered ? Theme.Accent : Theme.BorderStrong))
            g.DrawPath(pen, path);

        // Chevron (تنظیم موقعیت فلش بر اساس RTL)
        int cx = isRtl
            ? Theme.DpiScale(15, DeviceDpi)
            : Width - Theme.DpiScale(15, DeviceDpi);
        int cy = Height / 2;

        using var chevron = new Pen(_hovered ? Theme.Accent : Theme.TextSecondary, 1.6f);
        chevron.LineJoin = LineJoin.Round;
        int ch = Theme.DpiScale(4, DeviceDpi);
        PointF[] pts =
        {
            new(cx - ch, cy - Theme.DpiScale(2, DeviceDpi)),
            new(cx, cy + Theme.DpiScale(2, DeviceDpi)),
            new(cx + ch, cy - Theme.DpiScale(2, DeviceDpi))
        };
        g.DrawLines(chevron, pts);
    }
}
