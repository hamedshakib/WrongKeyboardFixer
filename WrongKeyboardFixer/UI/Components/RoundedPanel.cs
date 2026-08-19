using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     پنل تخت با گوشه‌های گرد. بدون Region:
///     ریجن‌ها کندند، فرزندان را clip می‌کنند و هندل GDI نشت می‌دهند.
/// </summary>
public class RoundedPanel : Panel
{
    private Color _borderColor = Theme.Border;
    private int _borderWidth = 1;
    private int _cornerRadius = Constants.UI.RoundedPanelDefaultCornerRadius;

    public RoundedPanel()
    {
        // به‌جای فقط DoubleBuffered، همهٔ پرچم‌های ضدفلیکر:
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
        BackColor = Theme.Surface;
        Padding = new Padding(Constants.UI.RoundedPanelDefaultPadding);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(0, value);
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderWidth
    {
        get => _borderWidth;
        set
        {
            _borderWidth = Math.Max(0, value);
            Invalidate();
        }
    }

    // رنگ‌آمیزی پیش‌فرض پس‌زمینه حذف می‌شود تا فلیکر دو مرحله‌ای نداشته باشیم.
    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // گوشه‌ها رنگ والد را نشان می‌دهند (بدون نیاز به Region)
        g.Clear(Parent?.BackColor ?? Theme.Background);

        if (Width <= 1 || Height <= 1) return;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.RoundRect(bounds, Theme.DpiScale(_cornerRadius, DeviceDpi));

        using (var brush = new SolidBrush(BackColor))
        {
            g.FillPath(brush, path);
        }

        if (_borderWidth > 0)
        {
            float bw = Theme.DpiScaleF(_borderWidth, DeviceDpi);
            int inset = (int)Math.Ceiling(bw / 2f);
            var strokeRect = new Rectangle(0, 0, Width - 1, Height - 1);
            strokeRect.Inflate(-inset, -inset);
            using var strokePath = Theme.RoundRect(strokeRect,
                Math.Max(0, Theme.DpiScale(_cornerRadius, DeviceDpi) - inset));
            using var pen = new Pen(_borderColor, bw);
            g.DrawPath(pen, strokePath);
        }
    }
}