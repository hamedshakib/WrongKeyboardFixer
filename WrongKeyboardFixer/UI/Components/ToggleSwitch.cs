using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

public class ToggleSwitch : CheckBox
{
    private const int SwitchWidth = Constants.UI.ToggleSwitchWidth;
    private const int SwitchHeight = Constants.UI.ToggleSwitchHeight;

    public ToggleSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        Height = Constants.UI.StdTextBoxHeight;
        Font = Theme.BodyFont;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Background);

        int switchW = Theme.DpiScale(SwitchWidth, DeviceDpi);
        int switchH = Theme.DpiScale(SwitchHeight, DeviceDpi);
        bool rtl = RightToLeft == RightToLeft.Yes;

        int x = rtl ? Width - switchW : 0;
        var trackRect = new Rectangle(x, (Height - switchH) / 2, switchW, switchH);

        // ── Track ──
        Color trackColor = Enabled ? Checked ? Theme.Accent : Theme.BorderStrong : Theme.SurfaceMuted;
        using (var trackPath = Theme.RoundRect(trackRect, switchH / 2))
        {
            using var brush = new SolidBrush(trackColor);
            g.FillPath(brush, trackPath);
            if (Enabled && !Checked)
            {
                using var pen = new Pen(Theme.BorderStrong);
                g.DrawPath(pen, trackPath);
            }
        }

        // ── Knob (در RTL آینه می‌شود) ──
        int knobSize = switchH - Theme.DpiScale(Constants.UI.DpiKnobSizeOffset, DeviceDpi);
        int knobY = trackRect.Y + (switchH - knobSize) / 2;
        int onX = rtl
            ? trackRect.X + Theme.DpiScale(3, DeviceDpi)
            : trackRect.Right - knobSize - Theme.DpiScale(3, DeviceDpi);
        int offX = rtl
            ? trackRect.Right - knobSize - Theme.DpiScale(3, DeviceDpi)
            : trackRect.X + Theme.DpiScale(3, DeviceDpi);
        int knobX = Checked ? onX : offX;

        using (var knobPath = Theme.RoundRect(new Rectangle(knobX, knobY, knobSize, knobSize), knobSize / 2))
        {
            using var brush = new SolidBrush(Enabled
                ? Checked ? Theme.TextOnAccent : Theme.Surface
                : Theme.TextDisabled);
            g.FillPath(brush, knobPath);
        }

        // ── متن: در RTL باید کنار کلید بچسبد، نه به گوشهٔ مقابل ──
        int gap = Theme.DpiScale(Constants.UI.ToggleSwitchGap, DeviceDpi);
        int textW = Math.Max(0, Width - switchW - gap - 2);
        if (textW > 0 && !string.IsNullOrEmpty(Text))
        {
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            // RightToLeft فقط جهت خواندن است؛ برای چسبیدن متن به کنار کلید
            // در حالت RTL تراز Right لازم است (قرینهٔ Left در حالت LTR)
            flags |= rtl
                ? TextFormatFlags.RightToLeft | TextFormatFlags.Right
                : TextFormatFlags.Left;

            var textRect = new Rectangle(rtl ? 0 : switchW + gap, 0, textW, Height);
            TextRenderer.DrawText(g, Text, Font, textRect,
                Enabled ? Theme.TextPrimary : Theme.TextDisabled, flags);

            // خط‌چین فوکوس فقط دور خود متن (نه کل عرض کنترل)
            if (Focused && ShowFocusCues)
            {
                int tw = Math.Min(TextRenderer.MeasureText(Text, Font).Width, textW);
                int fx = rtl ? textRect.Right - tw : textRect.Left;
                ControlPaint.DrawFocusRectangle(g, new Rectangle(fx, 2, tw, Height - 4));
            }
        }
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}