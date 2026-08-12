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

public class KeyboardMappingsForm : Form, ICloseRequestHandler
{
    private readonly AppSettings _settings;

    // کپی محلی نگاشت‌ها: فقط هنگام فشردن «ذخیره» روی تنظیمات اصلی اعمال می‌شوند.
    // (قبلاً ویرایش سلول مستقیماً _settings را تغییر می‌داد و «انصراف» بی‌اثر بود)
    private Dictionary<char, char> _p2e;
    private Dictionary<char, char> _e2p;

    private DataGridView? _dataGridViewPersianToEnglish;
    private DataGridView? _dataGridViewEnglishToPersian;
    private ModernButton? _btnSave;
    private ModernButton? _btnCancel;
    private Label? _lblCountPersianToEnglish;
    private Label? _lblCountEnglishToPersian;

    private const int FormWidth = 920;
    private const int FormHeight = 600;

    // براش‌های ایستا: به‌جای ساخت Brush برای هر سلول در هر Paint
    private static readonly SolidBrush DeleteBgBrush = new(Theme.DangerSoft);
    private static readonly SolidBrush ResetBgBrush = new(Theme.WarningSoft);

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();
        _p2e = new Dictionary<char, char>(_settings.PersianToEnglishMap ?? new Dictionary<char, char>());
        _e2p = new Dictionary<char, char>(_settings.EnglishToPersianMap ?? new Dictionary<char, char>());

        Localization.SetLanguage(_settings.Language);

        this.SuspendLayout();
        InitializeControls();
        this.ResumeLayout(false);

        LoadMappings();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.Style |= 0x02000000;   // WS_CLIPCHILDREN  → پس‌زمینهٔ فرم زیر بچه‌ها repaint نمی‌شود
            cp.Style |= 0x04000000;   // WS_CLIPSIBLINGS → کنترل‌ها روی هم overwrite نمی‌کنند
            return cp;
        }
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

        var titleBar = new ModernTitleBar
        {
            Dock = DockStyle.Top,
            Text = Localization.Get("KeyboardMappingsTitle"),
            Subtitle = Localization.Get("KeyboardMappingsSubtitle")
        };
        this.Controls.Add(titleBar);

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
        var card1 = new RoundedPanel
        {
            CornerRadius = 12,
            BackColor = Theme.SurfaceAlt,
            BorderColor = Theme.Border,
            BorderWidth = 1,
            Padding = new Padding(14),
            Location = new Point(left, cardY),
            Size = new Size(cardWidth, BuildCardHeight(gridHeight))
        };
        panel.Controls.Add(card1);

        card1.Controls.Add(Theme.SectionLabel(Localization.Get("PersianToEnglish"))
            .Then(l => l.Location = new Point(14, 12)));

        _lblCountPersianToEnglish = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _lblCountPersianToEnglish.Location = new Point(cardWidth - 14 - 130, 14);
        _lblCountPersianToEnglish.Size = new Size(130, 22);
        _lblCountPersianToEnglish.TextAlign = ContentAlignment.MiddleRight;
        _lblCountPersianToEnglish.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card1.Controls.Add(_lblCountPersianToEnglish);

        var addP2E = new ModernButton
        {
            Text = "+ " + Localization.Get("AddNewMapping"),   // گلیف ＋ حذف شد (در Segoe UI نبود)
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(14, 46),
            Size = new Size(160, 38)
        };
        addP2E.Click += BtnAddPersianToEnglish_Click;
        card1.Controls.Add(addP2E);

        var resetP2E = new ModernButton
        {
            Text = Localization.Get("ResetAll"),               // گلیف ↻ حذف شد
            ButtonVariant = ModernButton.Variant.Ghost,
            Location = new Point(14 + 160 + 10, 46),
            Size = new Size(170, 38)
        };
        resetP2E.Click += BtnResetAllPersianToEnglish_Click;
        card1.Controls.Add(resetP2E);

        _dataGridViewPersianToEnglish = CreateMappingGrid(true);
        _dataGridViewPersianToEnglish.Location = new Point(14, 94);
        _dataGridViewPersianToEnglish.Size = new Size(cardWidth - 28, gridHeight);
        card1.Controls.Add(_dataGridViewPersianToEnglish);

        // ── کارت انگلیسی → فارسی ──────────────────────────
        var card2 = new RoundedPanel
        {
            CornerRadius = 12,
            BackColor = Theme.SurfaceAlt,
            BorderColor = Theme.Border,
            BorderWidth = 1,
            Padding = new Padding(14),
            Location = new Point(left + cardWidth + cardGap, cardY),
            Size = new Size(cardWidth, BuildCardHeight(gridHeight))
        };
        panel.Controls.Add(card2);

        card2.Controls.Add(Theme.SectionLabel(Localization.Get("EnglishToPersian"))
            .Then(l => l.Location = new Point(14, 12)));

        _lblCountEnglishToPersian = Theme.BodyLabel("", Theme.TextSecondary, Theme.SmallFont);
        _lblCountEnglishToPersian.Location = new Point(cardWidth - 14 - 130, 14);
        _lblCountEnglishToPersian.Size = new Size(130, 22);
        _lblCountEnglishToPersian.TextAlign = ContentAlignment.MiddleRight;
        _lblCountEnglishToPersian.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        card2.Controls.Add(_lblCountEnglishToPersian);

        var addE2P = new ModernButton
        {
            Text = "+ " + Localization.Get("AddNewMapping"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(14, 46),
            Size = new Size(160, 38)
        };
        addE2P.Click += BtnAddEnglishToPersian_Click;
        card2.Controls.Add(addE2P);

        var resetE2P = new ModernButton
        {
            Text = Localization.Get("ResetAll"),
            ButtonVariant = ModernButton.Variant.Ghost,
            Location = new Point(14 + 160 + 10, 46),
            Size = new Size(170, 38)
        };
        resetE2P.Click += BtnResetAllEnglishToPersian_Click;
        card2.Controls.Add(resetE2P);

        _dataGridViewEnglishToPersian = CreateMappingGrid(false);
        _dataGridViewEnglishToPersian.Location = new Point(14, 94);
        _dataGridViewEnglishToPersian.Size = new Size(cardWidth - 28, gridHeight);
        card2.Controls.Add(_dataGridViewEnglishToPersian);

        // ── فوتر ──────────────────────────────────────────
        int footerY = panel.Height - panel.Padding.Bottom - 42;

        _btnCancel = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Location = new Point(right - 112, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _btnCancel.Click += BtnCancel_Click;
        panel.Controls.Add(_btnCancel);

        _btnSave = new ModernButton
        {
            Text = Localization.Get("Save"),
            ButtonVariant = ModernButton.Variant.Primary,
            Location = new Point(right - 112 - 124, footerY),
            Size = new Size(112, 38),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        _btnSave.Click += BtnSave_Click;
        panel.Controls.Add(_btnSave);

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
        var grid = new DataGridView();
        Theme.StyleGrid(grid);
        Theme.EnableDoubleBuffered(grid);

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
            grid.CellContentClick += DataGridViewPersianToEnglish_CellContentClick;
            grid.CellValidating += DataGridViewPersianToEnglish_CellValidating;
            grid.CellValueChanged += DataGridViewPersianToEnglish_CellValueChanged;
        }
        else
        {
            grid.CellContentClick += DataGridViewEnglishToPersian_CellContentClick;
            grid.CellValidating += DataGridViewEnglishToPersian_CellValidating;
            grid.CellValueChanged += DataGridViewEnglishToPersian_CellValueChanged;
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
        _dataGridViewPersianToEnglish.ClearSelection();
        _dataGridViewPersianToEnglish.CurrentCell = null;
        _dataGridViewEnglishToPersian.ClearSelection();
        _dataGridViewEnglishToPersian.CurrentCell = null;

        FillGrid(_dataGridViewPersianToEnglish!, _p2e);
        FillGrid(_dataGridViewEnglishToPersian!, _e2p);

        _lblCountPersianToEnglish!.Text = Localization.Format("MappingsCount", _p2e.Count);
        _lblCountEnglishToPersian!.Text = Localization.Format("MappingsCount", _e2p.Count);
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
    private void DataGridViewPersianToEnglish_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _dataGridViewPersianToEnglish!.Rows[e.RowIndex];
        row.ErrorText = string.Empty;
        if (row.Cells[0].Value is string pStr && pStr.Length == 1 &&
            row.Cells[1].Value is string eStr && eStr.Length == 1)
        {
            _p2e[pStr[0]] = eStr[0];
        }
    }

    private void DataGridViewEnglishToPersian_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _dataGridViewEnglishToPersian!.Rows[e.RowIndex];
        row.ErrorText = string.Empty;
        if (row.Cells[0].Value is string eStr && eStr.Length == 1 &&
            row.Cells[1].Value is string pStr && pStr.Length == 1)
        {
            _e2p[eStr[0]] = pStr[0];
        }
    }

    private void DataGridViewPersianToEnglish_CellContentClick(object? sender, DataGridViewCellEventArgs e)
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

    private void DataGridViewEnglishToPersian_CellContentClick(object? sender, DataGridViewCellEventArgs e)
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

    private void BtnAddPersianToEnglish_Click(object? sender, EventArgs e) => AddPersianToEnglishMapping();
    private void BtnAddEnglishToPersian_Click(object? sender, EventArgs e) => AddEnglishToPersianMapping();

    private void BtnResetAllPersianToEnglish_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _p2e = MappingDefaults.GetDefaultPersianToEnglishMap();
        LoadMappings();
    }

    private void BtnResetAllEnglishToPersian_Click(object? sender, EventArgs e)
    {
        if (ConfirmReset() != DialogResult.Yes) return;
        _e2p = MappingDefaults.GetDefaultEnglishToPersianMap();
        LoadMappings();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        // فقط حالا تغییرات روی تنظیمات اصلی اعمال می‌شود
        _settings.PersianToEnglishMap = new Dictionary<char, char>(_p2e);
        _settings.EnglishToPersianMap = new Dictionary<char, char>(_e2p);
        SettingsManager.Save(_settings);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public void RequestClose() => BtnCancel_Click(this, EventArgs.Empty);

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
        _e2p.Remove(en);
        _p2e[p] = en;
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
        _p2e.Remove(p);
        _e2p[en] = p;
        LoadMappings();
    }

    private void DeletePersianMapping(char persianChar)
    {
        if (_p2e.Remove(persianChar)) LoadMappings();
    }

    private void DeleteEnglishMapping(char englishChar)
    {
        if (_e2p.Remove(englishChar)) LoadMappings();
    }

    private void ResetPersianCharMapping(char persianChar)
    {
        if (_p2e.ContainsKey(persianChar)) { _p2e[persianChar] = persianChar; LoadMappings(); }
    }

    private void ResetEnglishCharMapping(char englishChar)
    {
        if (_e2p.ContainsKey(englishChar)) { _e2p[englishChar] = englishChar; LoadMappings(); }
    }

    // ── اعتبارسنجی: بدون MessageBox؛ خطا روی خود ردیف نمایش داده می‌شود ──
    private void DataGridViewPersianToEnglish_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;   // ستون صفر فقط‌خواندنی است
        var row = _dataGridViewPersianToEnglish!.Rows[e.RowIndex];

        string? value = e.FormattedValue?.ToString();
        if (string.IsNullOrEmpty(value) || value.Length != 1 || value[0] < 32 || value[0] > 126)
        {
            e.Cancel = true;
            row.ErrorText = Localization.Get("InvalidEnglishChar");
        }
    }

    private void DataGridViewEnglishToPersian_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 1) return;
        var row = _dataGridViewEnglishToPersian!.Rows[e.RowIndex];

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

/// <summary>
/// دیالوگ واحد برای گرفتن دو کاراکتر (جایگزین دو InputBox متوالی).
/// </summary>
public static class MappingInputBox
{
    public static (string first, string second)? Show(string title, string prompt1, string prompt2)
    {
        using var form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(380, 250),
            BackColor = Theme.Surface,
            Font = Theme.BodyFont,
            RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No,
            RightToLeftLayout = true,
            AutoScaleDimensions = new SizeF(96F, 96F),
            AutoScaleMode = AutoScaleMode.Dpi
        };

        var lbl1 = new Label
        {
            Text = prompt1,
            AutoSize = true,
            Location = new Point(24, 18),
            ForeColor = Theme.TextPrimary
        };
        form.Controls.Add(lbl1);

        var txt1 = new TextBox
        {
            Location = new Point(24, 44),
            Size = new Size(316, 30),
            MaxLength = 1,
            TextAlign = HorizontalAlignment.Center,
            Font = Theme.TitleFont,
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SurfaceAlt
        };
        form.Controls.Add(txt1);

        var lbl2 = new Label
        {
            Text = prompt2,
            AutoSize = true,
            Location = new Point(24, 92),
            ForeColor = Theme.TextPrimary
        };
        form.Controls.Add(lbl2);

        var txt2 = new TextBox
        {
            Location = new Point(24, 118),
            Size = new Size(316, 30),
            MaxLength = 1,
            TextAlign = HorizontalAlignment.Center,
            Font = Theme.TitleFont,
            BorderStyle = BorderStyle.FixedSingle,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SurfaceAlt
        };
        form.Controls.Add(txt2);

        var okButton = new ModernButton
        {
            Text = Localization.Get("OK"),
            ButtonVariant = ModernButton.Variant.Primary,
            DialogResult = DialogResult.OK,
            Location = new Point(124, 166),
            Size = new Size(100, 38)
        };
        form.Controls.Add(okButton);

        var cancelButton = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            DialogResult = DialogResult.Cancel,
            Location = new Point(240, 166),
            Size = new Size(100, 38)
        };
        form.Controls.Add(cancelButton);

        form.Icon = IconLoader.GetIcon();                     // آیکون برنامه برای دیالوگ
        form.Shown += (_, _) => { txt1.Focus(); txt1.SelectAll(); };   // به‌جای فقط Focus

        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;
        form.Shown += (_, _) => txt1.Focus();

        return form.ShowDialog() == DialogResult.OK ? (txt1.Text, txt2.Text) : null;
    }
}