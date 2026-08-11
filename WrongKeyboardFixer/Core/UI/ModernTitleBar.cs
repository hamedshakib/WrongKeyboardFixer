using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.Core.UI;

/// <summary>
/// A custom title bar with a rounded top border, drag support and the
/// standard minimize / close buttons.
/// </summary>
public class ModernTitleBar : Control
{
    private bool _hoveredMin;
    private bool _hoveredClose;
    private bool _pressedMin;
    private bool _pressedClose;

    public const int TitleBarHeight = 56;
    public const int ControlAreaWidth = 96;
    private const int ButtonWidth = 46;

    private int Scaled(int value) => Theme.DpiScale(value, DeviceDpi);
    private int BtnW => Scaled(ButtonWidth);
    private int CtrlW => Scaled(ControlAreaWidth);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Subtitle { get; set; } = string.Empty;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color SubtitleColor { get; set; } = Theme.TextSecondary;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color TitleColor { get; set; } = Theme.TextPrimary;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color IconColor { get; set; } = Theme.Accent;

    public ModernTitleBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        Height = TitleBarHeight;
        Font = Theme.TitleFont;
        Cursor = Cursors.Default;
    }

    private static TextFormatFlags Flags(string text) =>
        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
        (Theme.IsRtlText(text) ? TextFormatFlags.RightToLeft : (TextFormatFlags)0);

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        if (e.X >= Width - CtrlW)
        {
            _pressedMin = e.X < Width - BtnW;
            _pressedClose = !_pressedMin;
            Invalidate();
            return;
        }
        if (FindForm() is { } form)
        {
            if (form.WindowState == FormWindowState.Maximized)
            {
                var pt = form.PointToScreen(e.Location);
                form.WindowState = FormWindowState.Normal;
                form.Location = new Point(pt.X - form.Width / 2, pt.Y - 8);
            }
            form.BeginInvoke(() => Capture = false);
            form.BeginInvoke(() =>
            {
                ReleaseCapture();
                SendMessage(form.Handle, 0xA1, 0x2, IntPtr.Zero); // WM_NCLBUTTONDOWN, HTCAPTION
            });
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            if (_pressedClose && e.X >= Width - BtnW && FindForm() is { } form)
            {
                if (form is ICloseRequestHandler h) h.RequestClose();
                else form.Close();
            }
            else if (_pressedMin && e.X >= Width - CtrlW && FindForm() is { } f)
            {
                f.WindowState = FormWindowState.Minimized;
            }
            _pressedMin = _pressedClose = false;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool overMin = e.X >= Width - CtrlW && e.X < Width - BtnW;
        bool overClose = e.X >= Width - BtnW;
        if (overMin != _hoveredMin || overClose != _hoveredClose)
        {
            _hoveredMin = overMin;
            _hoveredClose = overClose;
            Invalidate();
        }
        Cursor = overMin || overClose ? Cursors.Hand : Cursors.Default;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoveredMin || _hoveredClose)
        {
            _hoveredMin = _hoveredClose = false;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Background);

        var top = Scaled(8);
        var r = new Rectangle(0, top, Width - 1, Height - top - 1);
        using var path = Theme.RoundRect(r, Scaled(12));
        using (var brush = new SolidBrush(Theme.Surface))
            g.FillPath(brush, path);

        // ── مهم: برش همهٔ رسم‌های بعدی (به‌ویژه hover قرمز دکمهٔ بستن)
        //    به مسیر گردِ نوار تا گوشهٔ مربعی بیرون نزند ──
        g.SetClip(path);

        int textStart = Scaled(34);
        int availW = Width - CtrlW - textStart;
        int startY;
        int titleH;

        if (string.IsNullOrEmpty(Subtitle))
        {
            titleH = TextRenderer.MeasureText(Text, Font).Height;
            startY = top + Math.Max(0, (Height - top - titleH) / 2);
            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(textStart, startY, availW, titleH), TitleColor, Flags(Text));
        }
        else
        {
            int tH = TextRenderer.MeasureText(Text, Font).Height;
            int sH = TextRenderer.MeasureText(Subtitle, Theme.SmallFont).Height;
            int gap = Scaled(3);
            int blockH = tH + gap + sH;
            startY = top + Math.Max(0, (Height - top - blockH) / 2);
            titleH = tH;

            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(textStart, startY, availW, tH), TitleColor, Flags(Text));
            TextRenderer.DrawText(g, Subtitle, Theme.SmallFont,
                new Rectangle(textStart, startY + tH + gap, availW, sH), SubtitleColor, Flags(Subtitle));
        }

        // ── نقطهٔ برند، هم‌تراز با خط عنوان ──
        int dotSize = Scaled(10);
        using (var dot = new SolidBrush(IconColor))
            g.FillEllipse(dot, Scaled(16), startY + titleH / 2f - dotSize / 2f, dotSize, dotSize);

        // ── دکمه‌ها (حالا داخل clip امن‌اند) ──
        DrawWindowButton(g, Width - (BtnW * 2), top, _hoveredMin, _pressedMin, isClose: false);
        DrawWindowButton(g, Width - BtnW, top, _hoveredClose, _pressedClose, isClose: true);

        g.ResetClip();
    }

    private void DrawWindowButton(Graphics g, int x, int top, bool hover, bool pressed, bool isClose)
    {
        var rect = new Rectangle(x, top, BtnW, Height - top);
        Color bg = pressed ? Theme.SurfaceMuted : hover && !isClose ? Theme.SurfaceAlt : hover ? Theme.Danger : Theme.Surface;
        using var brush = new SolidBrush(bg);
        g.FillRectangle(brush, rect);

        int cx = rect.X + rect.Width / 2;
        int cy = rect.Y + rect.Height / 2;

        if (isClose)
        {
            using var pen = new Pen(hover ? Theme.TextOnAccent : Theme.TextSecondary, 1.4f);
            int s = Scaled(8);
            g.DrawLine(pen, cx - s, cy - s, cx + s, cy + s);
            g.DrawLine(pen, cx + s, cy - s, cx - s, cy + s);
        }
        else
        {
            using var pen = new Pen(Theme.TextSecondary, 1.4f);
            g.DrawLine(pen, cx - Scaled(8), cy + Scaled(5), cx + Scaled(8), cy + Scaled(5));
            g.DrawLine(pen, cx - Scaled(5), cy, cx + Scaled(5), cy);
        }
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}

/// <summary>
/// Implemented by forms that own a <see cref="ModernTitleBar"/> so the close
/// button can trigger a custom shutdown flow.
/// </summary>
public interface ICloseRequestHandler
{
    void RequestClose();
}