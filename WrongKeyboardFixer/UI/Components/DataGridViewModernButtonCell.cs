using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
/// A DataGridView cell that renders a ModernButton-style button.
/// </summary>
public class DataGridViewModernButtonCell : DataGridViewButtonCell
{
    private bool _hovered;
    private bool _pressed;
    private int _cornerRadius = 7;

    public int CornerRadius
    {
        get => _cornerRadius;
        set => _cornerRadius = value;
    }

    public DataGridViewModernButtonCell() : base()
    {
        Style = new DataGridViewCellStyle
        {
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Font = Theme.ButtonFont,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.Surface,
            Padding = new Padding(0)
        };
    }

    private void InvalidateCell()
    {
        if (OwningRow is not null && OwningRow.DataGridView is not null)
        {
            OwningRow.DataGridView.InvalidateCell(this);
        }
    }

    protected override void OnMouseEnter(int rowIndex)
    {
        _hovered = true;
        InvalidateCell();
        base.OnMouseEnter(rowIndex);
    }

    protected override void OnMouseLeave(int rowIndex)
    {
        _hovered = false;
        _pressed = false;
        InvalidateCell();
        base.OnMouseLeave(rowIndex);
    }

    protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            InvalidateCell();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
    {
        _pressed = false;
        InvalidateCell();
        base.OnMouseUp(e);
    }

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
    {
        if (graphics is null)
            throw new ArgumentNullException(nameof(graphics));

        // Get DPI from the DataGridView if available
        int dpi = OwningRow?.DataGridView?.DeviceDpi ?? 96;

        var bounds = new Rectangle(cellBounds.X, cellBounds.Y, cellBounds.Width - 1, cellBounds.Height - 1);

        using var path = Theme.RoundRect(bounds, Theme.DpiScale(_cornerRadius, dpi));
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Use Theme.Surface as the base fill color (not cellStyle.BackColor which might be Black)
        Color fill = Theme.Surface;
        if (_hovered && !_pressed)
        {
            fill = Theme.SurfaceAlt;
        }
        if (_pressed)
        {
            fill = Theme.SurfaceMuted;
        }

        // Fill background
        using (var brush = new SolidBrush(fill))
            graphics.FillPath(brush, path);

        // Draw border
        using var borderPen = new Pen(Theme.BorderStrong);
        graphics.DrawPath(borderPen, path);

        // Draw text using TextRenderer for sharper Persian text
        if (!string.IsNullOrEmpty(formattedValue as string))
        {
            var text = formattedValue as string;
            Color textColor = Theme.TextPrimary;
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

            if (Theme.IsRtlText(text))
                flags |= TextFormatFlags.RightToLeft;

            TextRenderer.DrawText(graphics, text, Theme.ButtonFont, bounds, textColor, flags);
        }
    }
}