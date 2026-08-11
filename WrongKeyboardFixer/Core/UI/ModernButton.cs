using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

/// <summary>
/// A flat, rounded button that supports three variants: primary, secondary and ghost.
/// </summary>
public class ModernButton : Button
{
    public enum Variant
    {
        Primary,
        Secondary,
        Ghost
    }

    private Variant _variant = Variant.Primary;
    private bool _hovered;
    private bool _pressed;
    private int _cornerRadius = 7;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Variant ButtonVariant
    {
        get => _variant;
        set { _variant = value; UpdateColors(); Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    public ModernButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        Height = 36;
        Font = Theme.ButtonFont;
        UpdateColors();
    }

    private void UpdateColors()
    {
        BackColor = _variant == Variant.Primary ? Theme.Accent : Theme.Surface;
        ForeColor = _variant == Variant.Primary ? Theme.TextOnAccent : Theme.TextPrimary;
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); } base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Background);

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundRect(bounds, Theme.DpiScale(CornerRadius, DeviceDpi));

        Color fill = Enabled ? BackColor : Theme.SurfaceMuted;
        if (Enabled && _hovered)
            fill = _variant switch
            {
                Variant.Primary => Theme.AccentDark,
                Variant.Secondary => Theme.SurfaceAlt,
                _ => Theme.AccentSoft
            };
        if (Enabled && _pressed)
            fill = _variant switch
            {
                Variant.Primary => ControlPaint.Dark(Theme.AccentDark, 0.06f),
                _ => Theme.SurfaceMuted
            };

        using (var brush = new SolidBrush(fill))
            g.FillPath(brush, path);

        // ── حذف خط آبی زیر دکمه‌های Secondary ──
        // این خط باعث می‌شد دکمه Cancel همیشه یک خط آبی زیرش داشته باشد
        // که حس یک لینک یا خطای فوکوس را می‌داد. حذف آن UI را تمیزتر می‌کند.

        if (Enabled && _variant != Variant.Primary)
        {
            using var pen = new Pen(Theme.BorderStrong);
            g.DrawPath(pen, path);
        }

        // Border glow when hovered
        if (Enabled && _hovered && _variant != Variant.Secondary)
        {
            using var pen = new Pen(Theme.AccentBorder);
            g.DrawPath(pen, path);
        }

        // ── رسم متن با GDI (TextRenderer) به جای GDI+ (DrawString) ──
        // این کار باعث می‌شود متن‌ها (به‌ویژه فارسی) کاملاً شارپ و خوانا باشند و تار نشوند.
        if (!string.IsNullOrEmpty(Text))
        {
            Color textColor = Enabled ? ForeColor : Theme.TextDisabled;
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            // رعایت راست‌چین بودن برای متون فارسی/عربی
            if (Theme.IsRtlText(Text) || RightToLeft == RightToLeft.Yes)
                flags |= TextFormatFlags.RightToLeft;

            TextRenderer.DrawText(g, Text, Font, bounds, textColor, flags);
        }

        // ── رسم حلقه فوکوس فقط هنگام استفاده از کیبورد ──
        // با این کار هنگام کلیک موس خط چین زشتی دور دکمه نمی‌افتد.
        if (Focused && ShowFocusCues)
        {
            var focusBounds = bounds;
            focusBounds.Inflate(-4, -4);
            ControlPaint.DrawFocusRectangle(g, focusBounds);
        }
    }
}