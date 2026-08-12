using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.UI;

namespace WrongKeyboardFixer.UI.Forms;

public class KeyboardMappingsForm : ModernForm, ICloseRequestHandler
{
    private readonly AppSettings _settings;

    // کپی محلی نگاشت‌ها: فقط هنگام فشردن «ذخیره» روی تنظیمات اصلی اعمال می‌شوند.
    // (قبلاً ویرایش سلول مستقیماً _settings را تغییر می‌داد و «انصراف» بی‌اثر بود)
    private Dictionary<char, char> _persianToEnglish;
    private Dictionary<char, char> _englishToPersian;

    private DataGridView? _persianToEnglishGrid;
    private DataGridView? _englishToPersianGrid;
    private ModernButton? _saveButton;
    private ModernButton? _cancelButton;
    private Label? _persianToEnglishCountLabel;
    private Label? _englishToPersianCountLabel;

    private const int FormWidth = 920;
    private const int FormHeight = 600;

    // براش‌های ایستا: به‌جای ساخت Brush برای هر سلول در هر Paint
    private static readonly SolidBrush DeleteButtonBrush = new(Theme.DangerSoft);
    private static readonly SolidBrush ResetButtonBrush = new(Theme.WarningSoft);

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();
        _persianToEnglish = new Dictionary<char, char>(_settings.PersianToEnglishMap ?? new Dictionary<char, char>());
        _englishToPersian = new Dictionary<char, char>(_settings.EnglishToPersianMap ?? new Dictionary<char, char>());

        Localization.SetLanguage(_settings.Language);

        this.SuspendLayout();
        InitializeControls();
        this.ResumeLayout(false);

        LoadMappings();
    }

    private void InitializeControls()
    {
        this.Text = Localization.Get("KeyboardMappingsTitle");
        this.FormBorderStyle = FormBorderStyle.None;
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new Size(FormWidth, FormHeight);
        this.MinimumSize = new Size(FormWidth, FormHeight);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = Theme.BodyFont;
        this.Icon = IconLoader.GetIcon();
        this.RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        this.RightToLeftLayout = Localization.IsRtl;

        AddTitleBar("KeyboardMappingsTitle", "KeyboardMappingsSubtitle");

        var panel = new RoundedPanel
        {
            CornerRadius = 14,
            BackColor = Theme.Surface,
            Padding = new Padding(24),
            Location = new Point(14, ModernTitleBar.TitleBarHeight + 10),
            Size = new Size(FormWidth - 28, FormHeight - ModernTitleBar.TitleBarHeight - 24)
        };
        this.Controls.Add(panel);

        int left = panel.Padding.Left;
        int right = panel.Width - panel.Padding.Right;
        int innerWidth = right - left;
        const int cardGap = 16;
        int cardWidth = (innerWidth - cardGap) / 2;
        const int cardY = 14;
        const int gridHeight = 306;

        // ── کارت فارسی → انگلیسی ──────────────────────────
        _persianToEnglishCountLabel = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _persianToEnglishGrid = CreateMappingGrid(true);
        panel.Controls.Add(BuildMappingCard(
            left, cardWidth, cardY, gridHeight,
            "PersianToEnglish",
            _persianToEnglishCountLabel,
            _persianToEnglishGrid,
            AddPersianToEnglishButton_Click,
            ResetAllPersianToEnglishButton_Click));

        // ── کارت انگلیسی → فارسی ──────────────────────────
        _englishToPersianCountLabel = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _englishToPersianGrid = CreateMappingGrid(false);
        panel.Controls.Add(BuildMappingCard(
            left + cardWidth + cardGap, cardWidth, cardY, gridHeight,
            "EnglishToPersian",
            _englishToPersianCountLabel,
            _englishToPersianGrid,
            AddEnglishToPersianButton_Click,
            ResetAllEnglishToPersianButton_Click));

        // ── فوتر ──────────────────────────────────────────
        int footerY = panel.Height - panel.Padding.Bottom - 42;

        _cancelButton = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Location = new Point(right - 112, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _cancelButton.Click += CancelButton_Click;
        panel.Controls.Add(_cancelButton);

        _saveButton = new ModernButton
        {
            Text = Localization.Get("Save"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(right - 112 - 124, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _saveButton.Click += SaveButton_Click;
        panel.Controls.Add(_saveButton);

        var footerDivider = new RoundedPanel
        {
            Height = 1,
            Width = innerWidth,
            Location = new Point(left, footerY - 16),
            CornerRadius = 0,
            BackColor = Theme.Border,
            BorderWidth = 0,
            Padding = new Padding(0),
            TabStop = false,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        panel.Controls.Add(footerDivider);
        this.ActiveControl = null;
    }

    private static int BuildCardHeight(int gridHeight) => 14 + 26 + 6 + 38 + 10 + gridHeight + 14;

    /// <summary>
    /// Builds one mapping card (title, count label, add/reset buttons and the mapping grid).
    /// Shared by the Persian→English and English→Persian cards to avoid duplicated layout code.
    /// </summary>
    private RoundedPanel BuildMappingCard(
        int left, int cardWidth, int cardY, int gridHeight,
        string titleKey,
        Label countLabel,
        DataGridView grid,
        EventHandler addClick,
        EventHandler resetClick)
    {
        var card = new RoundedPanel
        {
            CornerRadius = 12,
            BackColor = Theme.SurfaceAlt,
            BorderColor = Theme.Border,
            BorderWidth = 1,
            Padding = new Padding(14),
            Location = new Point(left, cardY),
            Size = new Size(cardWidth, BuildCardHeight(gridHeight))
        };

        card.Controls.Add(Theme.SectionLabel(Localization.Get(titleKey))
            .Then(l => l.Location = new Point(14, 12)));

        countLabel.Location = new Point(cardWidth - 14 - 130, 14);
        countLabel.Size = new Size(130, 22);
        countLabel.TextAlign = ContentAlignment.MiddleRight;
        countLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card.Controls.Add(countLabel);

        var addButton = new ModernButton
        {
            Text = "+ " + Localization.Get("AddNewMapping"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(14, 46),
            Size = new Size(160, 38)
        };
        addButton.Click += addClick;
        card.Controls.Add(addButton);

        var resetButton = new ModernButton
        {
            Text = Localization.Get("ResetAll"),
            ButtonVariant = ModernButton.Variant.Ghost,
            Location = new Point(14 + 160 + 10, 46),
            Size = new Size(170, 38)
        };
        resetButton.Click += resetClick;
        card.Controls.Add(resetButton);

        grid.Location = new Point(14, 94);
        grid.Size = new Size(cardWidth - 28, gridHeight);
        card.Controls.Add(grid);

        return card;
    }

    private static bool IsButtonColumn(DataGridView grid, int col) =>
        col >= 0 && col < grid.Columns.Count && grid.Columns[col] is DataGridViewButtonColumn;

    private void Grid_MouseMove(object? sender, MouseEventArgs e)
    {
        var grid = (DataGridView)sender!;
        var st = (HoverState)grid.Tag!;
        var hit = grid.HitTest(e.X, e.Y);

        grid.Cursor = hit.RowIndex >= 0 && IsButtonColumn(grid, hit.ColumnIndex)
            ? Cursors.Hand : Cursors.Default;

        if (hit.RowIndex != st.Row || hit.ColumnIndex != st.Column)
        {
            int oldRow = st.Row;
            st.Row = hit.RowIndex;
            st.Column = hit.ColumnIndex;
            // بازترسیم کل ردیف قدیم و جدید → حاشیه‌ها هرگز نصفه نمی‌مانند
            if (oldRow >= 0 && oldRow < grid.Rows.Count) grid.InvalidateRow(oldRow);
            if (st.Row >= 0) grid.InvalidateRow(st.Row);
        }
    }

    private void Grid_MouseLeave(object? sender, EventArgs e)
    {
        var grid = (DataGridView)sender!;
        var st = (HoverState)grid.Tag!;
        int oldRow = st.Row;
        st.Row = st.Column = -1;
        st.Pressed = false;
        grid.Cursor = Cursors.Default;
        if (oldRow >= 0 && oldRow < grid.Rows.Count) grid.InvalidateRow(oldRow);
    }

    private void Grid_MouseDown(object? sender, MouseEventArgs e)
    {
        var grid = (DataGridView)sender!;
        var st = (HoverState)grid.Tag!;
        var hit = grid.HitTest(e.X, e.Y);
        if (hit.RowIndex >= 0 && IsButtonColumn(grid, hit.ColumnIndex))
        {
            st.Pressed = true;
            grid.InvalidateRow(hit.RowIndex);
        }
    }

    private void Grid_MouseUp(object? sender, MouseEventArgs e)
    {
        var grid = (DataGridView)sender!;
        var st = (HoverState)grid.Tag!;
        if (st.Pressed)
        {
            st.Pressed = false;
            if (st.Row >= 0 && st.Row < grid.Rows.Count) grid.InvalidateRow(st.Row);
        }
    }

    private DataGridView CreateMappingGrid(bool isPersianToEnglish)
    {
        var grid = new ModernDataGridView();
        Theme.StyleGrid(grid);

        // حاشیه‌های پیش‌فرض بدنه حذف می‌شوند؛ خطوط را خودمان یکنواخت می‌کشیم
        grid.CellBorderStyle = DataGridViewCellBorderStyle.None;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        string firstHeader = isPersianToEnglish
            ? Localization.Get("PersianChar")
            : Localization.Get("EnglishChar");
        string secondHeader = isPersianToEnglish
            ? Localization.Get("EnglishChar")
            : Localization.Get("PersianChar");

        var firstColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = firstHeader,
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 30,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                BackColor = Theme.SurfaceMuted,
                ForeColor = Theme.TextSecondary
            }
        };
        grid.Columns.Add(firstColumn);

        var secondColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = secondHeader,
            ReadOnly = false,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 30,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = Theme.Accent,
                Font = Theme.BodyBoldFont
            }
        };
        grid.Columns.Add(secondColumn);

        var deleteColumn = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Delete"),
            Text = Localization.Get("Delete"),
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        grid.Columns.Add(deleteColumn);

        var resetColumn = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Reset"),
            Text = Localization.Get("Reset"),
            UseColumnTextForButtonValue = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20,
            FlatStyle = FlatStyle.Flat,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        grid.Columns.Add(resetColumn);

        grid.CellPainting += Grid_CellPainting;
        grid.CellValidated += Grid_CellValidated;

        if (isPersianToEnglish)
        {
            grid.CellContentClick += PersianToEnglishGrid_CellContentClick;
            grid.CellValidating += PersianToEnglishGrid_CellValidating;
            grid.CellValueChanged += PersianToEnglishGrid_CellValueChanged;
        }
        else
        {
            grid.CellContentClick += EnglishToPersianGrid_CellContentClick;
            grid.CellValidating += EnglishToPersianGrid_CellValidating;
            grid.CellValueChanged += EnglishToPersianGrid_CellValueChanged;
        }

        grid.Tag = new HoverState();
        grid.ShowCellToolTips = false;      // حذف یک invalidate اضافی روی هاور
        grid.MouseMove += Grid_MouseMove;
        grid.MouseLeave += Grid_MouseLeave;
        grid.MouseDown += Grid_MouseDown;
        grid.MouseUp += Grid_MouseUp;
        return grid;
    }

    private sealed class HoverState
    {
        public int Row = -1;
        public int Column = -1;
        public bool Pressed;
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (sender is not DataGridView grid) return;
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;   // هدر: رسم پیش‌فرض

        var bounds = e.CellBounds;
        var st = (HoverState)grid.Tag!;
        var colStyle = grid.Columns[e.ColumnIndex].DefaultCellStyle;
        bool isButton = grid.Columns[e.ColumnIndex] is DataGridViewButtonColumn;
        bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
        bool rowHover = st.Row == e.RowIndex;
        bool cellHover = rowHover && st.Column == e.ColumnIndex && isButton;

        Color hoverColor = Color.FromArgb(232, 238, 246);
        Color bg = selected ? Theme.AccentSoft
            : rowHover ? hoverColor
            : colStyle.BackColor != Color.Empty ? colStyle.BackColor
            : e.RowIndex % 2 == 1 ? Color.FromArgb(250, 251, 253)
            : Theme.Surface;

        // کاهش ۱ پیکسلی ارتفاع باعث می‌شود پس‌زمینه هرگز روی خط حاشیه رسم نشود
        var bgBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height - 1);
        using (var bgBrush = new SolidBrush(bg))
            e.Graphics!.FillRectangle(bgBrush, bgBounds);

        // ── ۲. رسم محتوای داخلی سلول ──
        if (isButton)
        {
            bool isDelete = e.ColumnIndex == 2;
            var rect = bounds;
            rect.Inflate(-Theme.DpiScale(8, grid.DeviceDpi), -Theme.DpiScale(6, grid.DeviceDpi));
            if (rect.Width > 20 && rect.Height > 10)
            {
                Color pillBg = isDelete ? Theme.DangerSoft : Theme.WarningSoft;
                Color pillFg = isDelete ? Theme.Danger : Theme.Warning;
                if (cellHover)
                {
                    pillBg = isDelete ? Theme.Danger : Theme.Warning;
                    pillFg = Theme.TextOnAccent;
                    if (st.Pressed) pillBg = ControlPaint.Dark(pillBg, 0.12f);
                }

                // ذخیره وضعیت گرافیک برای جلوگیری از نشت AntiAlias به سایر سلول‌ها
                var prevSmoothing = e.Graphics.SmoothingMode;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using var path = Theme.RoundRect(
                    new Rectangle(rect.X, rect.Y, rect.Width - 1, rect.Height - 1),
                    Theme.DpiScale(7, grid.DeviceDpi));
                using (var pill = new SolidBrush(pillBg))
                    e.Graphics.FillPath(pill, path);

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
                TextRenderer.DrawText(e.Graphics, text, colStyle.Font ?? grid.Font, bounds, fore, flags);
            }
        }

        // ── ۳. خط مویی یکنواخت زیر کل ردیف ──
        var oldSmoothingForLine = e.Graphics.SmoothingMode;
        // اجبار به خاموش بودن AntiAlias برای رسم خطوط صاف و شارپ
        e.Graphics.SmoothingMode = SmoothingMode.None;
        using (var linePen = new Pen(grid.GridColor))
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


    private static void Grid_CellValidated(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && sender is DataGridView grid)
            grid.Rows[e.RowIndex].ErrorText = string.Empty;
    }

    // ── بارگذاری: با SuspendLayout و بدون انتخاب اضافی ──
    private void LoadMappings()
    {
        if (_persianToEnglishGrid is { } p2eGrid)
        {
            p2eGrid.ClearSelection();
            p2eGrid.CurrentCell = null;
            FillGrid(p2eGrid, _persianToEnglish);
            _persianToEnglishCountLabel!.Text = Localization.Format("MappingsCount", _persianToEnglish.Count);
        }

        if (_englishToPersianGrid is { } e2pGrid)
        {
            e2pGrid.ClearSelection();
            e2pGrid.CurrentCell = null;
            FillGrid(e2pGrid, _englishToPersian);
            _englishToPersianCountLabel!.Text = Localization.Format("MappingsCount", _englishToPersian.Count);
        }
    }

    private static void FillGrid(DataGridView grid, Dictionary<char, char> map)
    {
        if (grid.Tag is HoverState st) { st.Row = st.Column = -1; st.Pressed = false; }
        grid.SuspendLayout();
        grid.Rows.Clear();
        foreach (var key in map.Keys.OrderBy(c => c))
            grid.Rows.Add(key.ToString(), map[key].ToString());
        grid.ResumeLayout();
        grid.ClearSelection();
        grid.CurrentCell = null;
    }

    // ── ویرایش سلول → فقط کپی محلی ──
    private void PersianToEnglishGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _persianToEnglishGrid!.Rows[e.RowIndex];
        row.ErrorText = string.Empty;
        if (row.Cells[0].Value is string pStr && pStr.Length == 1 &&
            row.Cells[1].Value is string eStr && eStr.Length == 1)
        {
            _persianToEnglish[pStr[0]] = eStr[0];
        }
    }

    private void EnglishToPersianGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _englishToPersianGrid!.Rows[e.RowIndex];
        row.ErrorText = string.Empty;
        if (row.Cells[0].Value is string eStr && eStr.Length == 1 &&
            row.Cells[1].Value is string pStr && pStr.Length == 1)
        {
            _englishToPersian[eStr[0]] = pStr[0];
        }
    }

    private void PersianToEnglishGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        var dataGridView = (DataGridView)sender!;
        string? charStr = dataGridView.Rows[e.RowIndex].Cells[0].Value?.ToString();
        if (string.IsNullOrEmpty(charStr) || charStr.Length != 1) return;

        if (e.ColumnIndex == 2 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            DeletePersianMapping(charStr[0]);
        else if (e.ColumnIndex == 3 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            ResetPersianCharMapping(charStr[0]);
    }

    private void EnglishToPersianGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        var dataGridView = (DataGridView)sender!;
        string? charStr = dataGridView.Rows[e.RowIndex].Cells[0].Value?.ToString();
        if (string.IsNullOrEmpty(charStr) || charStr.Length != 1) return;

        if (e.ColumnIndex == 2 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            DeleteEnglishMapping(charStr[0]);
        else if (e.ColumnIndex == 3 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            ResetEnglishCharMapping(charStr[0]);
    }

    private void AddPersianToEnglishButton_Click(object? sender, EventArgs e) => AddPersianToEnglishMapping();
    private void AddEnglishToPersianButton_Click(object? sender, EventArgs e) => AddEnglishToPersianMapping();

    private void ResetAllPersianToEnglishButton_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _persianToEnglish = MappingDefaults.GetDefaultPersianToEnglishMap();
        LoadMappings();
    }

    private void ResetAllEnglishToPersianButton_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _englishToPersian = MappingDefaults.GetDefaultEnglishToPersianMap();
        LoadMappings();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        // فقط حالا تغییرات روی تنظیمات اصلی اعمال می‌شود
        _settings.PersianToEnglishMap = new Dictionary<char, char>(_persianToEnglish);
        _settings.EnglishToPersianMap = new Dictionary<char, char>(_englishToPersian);
        SettingsManager.Save(_settings);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public void RequestClose() => CancelButton_Click(this, EventArgs.Empty);

    // ── افزودن نگاشت: فقط یک دیالوگ (به‌جای دو InputBox پشت‌سرهم) ──
    private void AddPersianToEnglishMapping()
    {
        var input = MappingInputBox.Show(
            Localization.Get("AddMapping"),
            Localization.Get("EnterPersianChar"),
            Localization.Get("EnterEnglishChar"));
        if (input is null) return;

        string persian = input.Value.first.Trim();
        string english = input.Value.second.Trim();

        if (persian.Length != 1) { ShowError("InvalidPersianChar"); return; }
        if (english.Length != 1 || english[0] < 32 || english[0] > 126) { ShowError("InvalidEnglishChar"); return; }

        char p = persian[0], en = english[0];
        _englishToPersian.Remove(en);
        _persianToEnglish[p] = en;
        LoadMappings();
    }

    private void AddEnglishToPersianMapping()
    {
        var input = MappingInputBox.Show(
            Localization.Get("AddMapping"),
            Localization.Get("EnterEnglishChar"),
            Localization.Get("EnterPersianChar"));
        if (input is null) return;

        string english = input.Value.first.Trim();
        string persian = input.Value.second.Trim();

        if (english.Length != 1 || english[0] < 32 || english[0] > 126) { ShowError("InvalidEnglishChar"); return; }
        if (persian.Length != 1) { ShowError("InvalidPersianChar"); return; }

        char en = english[0], p = persian[0];
        _persianToEnglish.Remove(p);
        _englishToPersian[en] = p;
        LoadMappings();
    }

    private void DeletePersianMapping(char persianChar)
    {
        if (_persianToEnglish.Remove(persianChar)) LoadMappings();
    }

    private void DeleteEnglishMapping(char englishChar)
    {
        if (_englishToPersian.Remove(englishChar)) LoadMappings();
    }

    private void ResetPersianCharMapping(char persianChar)
    {
        if (_persianToEnglish.Remove(persianChar)) LoadMappings();
    }

    private void ResetEnglishCharMapping(char englishChar)
    {
        if (_englishToPersian.Remove(englishChar)) LoadMappings();
    }

    // ── اعتبارسنجی: بدون MessageBox؛ خطا روی خود ردیف نمایش داده می‌شود ──
    private void PersianToEnglishGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;   // ستون صفر فقط‌خواندنی است
        var row = _persianToEnglishGrid!.Rows[e.RowIndex];

        string? value = e.FormattedValue?.ToString();
        if (string.IsNullOrEmpty(value) || value.Length != 1 || value[0] < 32 || value[0] > 126)
        {
            e.Cancel = true;
            row.ErrorText = Localization.Get("InvalidEnglishChar");
        }
    }

    private void EnglishToPersianGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _englishToPersianGrid!.Rows[e.RowIndex];

        string? value = e.FormattedValue?.ToString();
        if (string.IsNullOrEmpty(value) || value.Length != 1)
        {
            e.Cancel = true;
            row.ErrorText = Localization.Get("InvalidPersianChar");
        }
    }

    private static DialogResult ConfirmReset() => MessageBox.Show(
        Localization.Get("ResetAllConfirm"),
        Localization.Get("Attention"),
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

    private static void ShowError(string key) => MessageBox.Show(
        Localization.Get(key),
        Localization.Get("Error"),
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}
