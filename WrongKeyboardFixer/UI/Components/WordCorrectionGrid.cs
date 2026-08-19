using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Components;

/// <summary>
///     Editable table for word corrections (آ / ژ) with Delete pill button.
///     Matches the styling of MappingGrid with pill-style buttons.
/// </summary>
public sealed class WordCorrectionGrid : ModernDataGridView
{
    private readonly HoverState _hover = new();

    public WordCorrectionGrid()
    {
        Build();
    }

    /// <summary>Raised when the Delete pill of a row is clicked.</summary>
    public event Action<string>? DeleteRequested;

    private void Build()
    {
        Theme.StyleGrid(this);

        // حاشیه‌های پیش‌فرض بدنه حذف می‌شوند؛ خطوط را خودمان یکنواخت می‌کشیم
        CellBorderStyle = DataGridViewCellBorderStyle.None;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        // ستون کلمه (فقط خواندنی)
        Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = Localization.Get("Word"),
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = Constants.UI.GridColumnLarge,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = Theme.TextSecondary
            }
        });

        // ستون دکمه Delete (pill button)
        Columns.Add(new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Delete"),
            Text = Localization.Get("Delete"),
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = Constants.UI.GridColumnLarge,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        CellValidated += (_, e) =>
        {
            if (e.RowIndex >= 0) Rows[e.RowIndex].ErrorText = string.Empty;
        };
        CellValueChanged += OnCellValueChanged;
        CellContentClick += OnCellContentClick;
        CellPainting += OnCellPainting;

        ShowCellToolTips = false;
        MouseMove += OnGridMouseMove;
        MouseLeave += OnGridMouseLeave;
        MouseDown += OnGridMouseDown;
        MouseUp += OnGridMouseUp;
    }

    /// <summary>
    ///     Replaces the grid content with the given words, sorted alphabetically.
    /// </summary>
    public void Load(IEnumerable<string> words)
    {
        _hover.Reset();
        SuspendLayout();
        Rows.Clear();
        foreach (var word in words.OrderBy(w => w))
            Rows.Add(word, Localization.Get("Delete"));
        ResumeLayout();
        ClearSelection();
        CurrentCell = null;
    }

    private void OnCellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        // For future use if needed
    }

    private void OnCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        if (Rows[e.RowIndex].Cells[0].Value is not string word) return;

        if (e.ColumnIndex == 1 && Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            DeleteRequested?.Invoke(word);
    }

    // ── حالت هاور ──

    private static bool IsButtonColumn(DataGridView grid, int col)
    {
        return col >= 0 && col < grid.Columns.Count && grid.Columns[col] is DataGridViewButtonColumn;
    }

    private void OnGridMouseMove(object? sender, MouseEventArgs e)
    {
        var hit = HitTest(e.X, e.Y);

        Cursor = hit.RowIndex >= 0 && IsButtonColumn(this, hit.ColumnIndex)
            ? Cursors.Hand
            : Cursors.Default;

        if (hit.RowIndex != _hover.Row || hit.ColumnIndex != _hover.Column)
        {
            int oldRow = _hover.Row;
            _hover.Row = hit.RowIndex;
            _hover.Column = hit.ColumnIndex;
            // بازترسیم کل ردیف قدیم و جدید → حاشیه‌ها هرگز نصفه نمی‌مانند
            if (oldRow >= 0 && oldRow < Rows.Count) InvalidateRow(oldRow);
            if (_hover.Row >= 0) InvalidateRow(_hover.Row);
        }
    }

    private void OnGridMouseLeave(object? sender, EventArgs e)
    {
        int oldRow = _hover.Row;
        _hover.Reset();
        Cursor = Cursors.Default;
        if (oldRow >= 0 && oldRow < Rows.Count) InvalidateRow(oldRow);
    }

    private void OnGridMouseDown(object? sender, MouseEventArgs e)
    {
        var hit = HitTest(e.X, e.Y);
        if (hit.RowIndex >= 0 && IsButtonColumn(this, hit.ColumnIndex))
        {
            _hover.Pressed = true;
            InvalidateRow(hit.RowIndex);
        }
    }

    private void OnGridMouseUp(object? sender, MouseEventArgs e)
    {
        if (_hover.Pressed)
        {
            _hover.Pressed = false;
            if (_hover.Row >= 0 && _hover.Row < Rows.Count) InvalidateRow(_hover.Row);
        }
    }

    // ── نقاشی سلول ──

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return; // هدر: رسم پیش‌فرض

        var bounds = e.CellBounds;
        var colStyle = Columns[e.ColumnIndex].DefaultCellStyle;
        bool isButton = Columns[e.ColumnIndex] is DataGridViewButtonColumn;
        bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
        bool rowHover = _hover.Row == e.RowIndex;
        bool cellHover = rowHover && _hover.Column == e.ColumnIndex && isButton;

        Color hoverColor = Theme.GridRowHover;
        Color bg = selected ? Theme.AccentSoft
            : rowHover ? hoverColor
            : colStyle.BackColor != Color.Empty ? colStyle.BackColor
            : e.RowIndex % 2 == 1 ? Theme.GridAlternatingRow
            : Theme.Surface;

        // کاهش ۱ پیکسلی ارتفاع باعث می‌شود پس‌زمینه هرگز روی خط حاشیه رسم نشود
        var bgBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 1);
        using (var bgBrush = new SolidBrush(bg))
        {
            e.Graphics!.FillRectangle(bgBrush, bgBounds);
        }

        // ── ۲. رسم محتوای داخلی سلول ──
        if (isButton)
        {
            var rect = bounds;
            rect.Inflate(-Theme.DpiScale(Constants.UI.DpiTrackPadding, DeviceDpi), -Theme.DpiScale(Constants.UI.DpiCellPadding, DeviceDpi));
            if (rect.Width > 20 && rect.Height > 10)
            {
                Color pillBg = Theme.DangerSoft;
                Color pillFg = Theme.Danger;
                if (cellHover)
                {
                    pillBg = Theme.Danger;
                    pillFg = Theme.TextOnAccent;
                    if (_hover.Pressed) pillBg = ControlPaint.Dark(pillBg, 0.12f);
                }

                // ذخیره وضعیت گرافیک برای جلوگیری از نشت AntiAlias به سایر سلول‌ها
                var prevSmoothing = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using var path = Theme.RoundRect(
                    new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1),
                    Theme.DpiScale(Constants.UI.CornerRadius, DeviceDpi));
                using (var pill = new SolidBrush(pillBg))
                {
                    e.Graphics.FillPath(pill, path);
                }

                // برگرداندن وضعیت به حالت قبل
                e.Graphics.SmoothingMode = prevSmoothing;

                string text = e.FormattedValue?.ToString() ?? "";
                var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                if (Theme.IsRtlText(text)) flags |= TextFormatFlags.RightToLeft;
                TextRenderer.DrawText(e.Graphics, text, Theme.BodyBoldFont, rect, pillFg, flags);
            }
        }
        else
        {
            string? text = e.FormattedValue?.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                if (Theme.IsRtlText(text)) flags |= TextFormatFlags.RightToLeft;
                Color fore = selected || rowHover ? Theme.TextPrimary
                    : colStyle.ForeColor != Color.Empty ? colStyle.ForeColor : Theme.TextPrimary;
                TextRenderer.DrawText(e.Graphics, text, colStyle.Font ?? Font, bounds, fore, flags);
            }
        }

        // ── ۳. خط مویی یکنواخت زیر کل ردیف ──
        var oldSmoothingForLine = e.Graphics.SmoothingMode;
        // اجبار به خاموش بودن AntiAlias برای رسم خطوط صاف و شارپ
        e.Graphics.SmoothingMode = SmoothingMode.None;
        using (var linePen = new Pen(GridColor))
        {
            // استفاده از bounds.Right (بدون منفی یک) برای اتصال کامل خطوط ستون‌ها به هم
            e.Graphics.DrawLine(linePen, bounds.Left, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
        }

        e.Graphics.SmoothingMode = oldSmoothingForLine;

        // ── ۴. حلقهٔ فوکوس کیبورد ──
        if ((e.State & DataGridViewElementStates.Selected) != 0)
        {
            using var focusPen = new Pen(Theme.AccentBorder);
            e.Graphics.DrawRectangle(focusPen,
                bounds.X + 1, bounds.Y + 1, bounds.Width - 3, bounds.Height - 3);
        }

        e.Handled = true;
    }
}