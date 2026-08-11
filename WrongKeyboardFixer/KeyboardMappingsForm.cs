using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public class KeyboardMappingsForm : Form
{
    private readonly AppSettings _settings;

    private DataGridView? _dataGridViewPersianToEnglish;
    private DataGridView? _dataGridViewEnglishToPersian;

    private Button? _btnAddPersianToEnglish;
    private Button? _btnAddEnglishToPersian;

    private Button? _btnResetAllPersianToEnglish;
    private Button? _btnResetAllEnglishToPersian;

    private Button? _btnClose;

    private Label? _titleLabel;
    private Label? _labelPersianToEnglish;
    private Label? _labelEnglishToPersian;

    private const int FormWidth = 700;

    private const int Margin = 20;

    private const int GridHeight = 165;

    private const int ButtonHeight = 34;

    private const int SectionSpacing = 18;

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();

        _settings.PersianToEnglishMap ??=
            new Dictionary<char, char>();

        _settings.EnglishToPersianMap ??=
            new Dictionary<char, char>();

        Localization.SetLanguage(_settings.Language);

        InitializeControls();

        LoadMappings();
    }

    // =========================================================
    // FORM
    // =========================================================

    private void InitializeControls()
    {
        this.Text =
            Localization.Get("KeyboardMappingsTitle");

        this.ClientSize =
            new Size(FormWidth, 610);

        this.FormBorderStyle =
            FormBorderStyle.FixedDialog;

        this.MaximizeBox = false;
        this.MinimizeBox = false;

        this.StartPosition =
            FormStartPosition.CenterScreen;

        this.Font =
            new Font("Tahoma", 9);

        this.Icon =
            IconLoader.GetIcon();

        this.RightToLeft =
            Localization.IsRtl
                ? RightToLeft.Yes
                : RightToLeft.No;

        this.RightToLeftLayout =
            Localization.IsRtl;

        int contentWidth =
            FormWidth - (Margin * 2);

        int currentY = 15;

        // =====================================================
        // TITLE
        // =====================================================

        _titleLabel = new Label
        {
            Text =
                Localization.Get(
                    "KeyboardMappingsTitle"),

            Font = new Font(
                "Tahoma",
                12,
                FontStyle.Bold),

            Location =
                new Point(Margin, currentY),

            Size =
                new Size(contentWidth, 32),

            TextAlign =
                ContentAlignment.MiddleLeft
        };

        Controls.Add(_titleLabel);

        currentY += 42;

        // =====================================================
        // PERSIAN -> ENGLISH
        // =====================================================

        _labelPersianToEnglish = CreateSectionTitle(
            Localization.Get("PersianToEnglish"));

        _labelPersianToEnglish.Location =
            new Point(Margin, currentY);

        Controls.Add(_labelPersianToEnglish);

        currentY += 30;

        ////Info box
        //var persianToEnglishInfo =
        //    CreateInfoLabel(
        //        "ⓘ  Persian characters (left column) are read-only. " +
        //        "You can change the English character (second column).");

        //persianToEnglishInfo.Location =
        //    new Point(Margin, currentY);

        //Controls.Add(persianToEnglishInfo);

        //currentY += 36;

        // Grid
        _dataGridViewPersianToEnglish =
            CreateMappingGrid(true);

        _dataGridViewPersianToEnglish.Location =
            new Point(Margin, currentY);

        _dataGridViewPersianToEnglish.Size =
            new Size(contentWidth, GridHeight);

        Controls.Add(
            _dataGridViewPersianToEnglish);

        currentY += GridHeight + 8;

        // Buttons
        _btnAddPersianToEnglish =
            CreateActionButton(
                "＋ " +
                Localization.Get("AddMapping"),
                Color.LightGreen);

        _btnAddPersianToEnglish.Location =
            new Point(Margin, currentY);

        _btnAddPersianToEnglish.Size =
            new Size(140, ButtonHeight);

        _btnAddPersianToEnglish.Click +=
            BtnAddPersianToEnglish_Click;

        Controls.Add(_btnAddPersianToEnglish);

        _btnResetAllPersianToEnglish =
            CreateActionButton(
                "↻ " +
                Localization.Get("ResetAll"),
                Color.Orange);

        _btnResetAllPersianToEnglish.Size =
            new Size(160, ButtonHeight);

        _btnResetAllPersianToEnglish.Location =
            new Point(
                FormWidth -
                Margin -
                _btnResetAllPersianToEnglish.Width,
                currentY);

        _btnResetAllPersianToEnglish.Click +=
            BtnResetAllPersianToEnglish_Click;

        Controls.Add(
            _btnResetAllPersianToEnglish);

        currentY +=
            ButtonHeight +
            SectionSpacing;

        // =====================================================
        // ENGLISH -> PERSIAN
        // =====================================================

        _labelEnglishToPersian =
            CreateSectionTitle(
                Localization.Get(
                    "EnglishToPersian"));

        _labelEnglishToPersian.Location =
            new Point(Margin, currentY);

        Controls.Add(_labelEnglishToPersian);

        currentY += 30;

        //// Info box
        //var englishToPersianInfo =
        //    CreateInfoLabel(
        //        "ⓘ  English characters (left column) are read-only. " +
        //        "You can change the Persian character (second column).");

        //englishToPersianInfo.Location =
        //    new Point(Margin, currentY);

        //Controls.Add(englishToPersianInfo);

        //currentY += 36;

        // Grid
        _dataGridViewEnglishToPersian =
            CreateMappingGrid(false);

        _dataGridViewEnglishToPersian.Location =
            new Point(Margin, currentY);

        _dataGridViewEnglishToPersian.Size =
            new Size(contentWidth, GridHeight);

        Controls.Add(
            _dataGridViewEnglishToPersian);

        currentY += GridHeight + 8;

        // Buttons
        _btnAddEnglishToPersian =
            CreateActionButton(
                "＋ " +
                Localization.Get("AddMapping"),
                Color.LightGreen);

        _btnAddEnglishToPersian.Location =
            new Point(Margin, currentY);

        _btnAddEnglishToPersian.Size =
            new Size(140, ButtonHeight);

        _btnAddEnglishToPersian.Click +=
            BtnAddEnglishToPersian_Click;

        Controls.Add(
            _btnAddEnglishToPersian);

        _btnResetAllEnglishToPersian =
            CreateActionButton(
                "↻ " +
                Localization.Get("ResetAll"),
                Color.Orange);

        _btnResetAllEnglishToPersian.Size =
            new Size(160, ButtonHeight);

        _btnResetAllEnglishToPersian.Location =
            new Point(
                FormWidth -
                Margin -
                _btnResetAllEnglishToPersian.Width,
                currentY);

        _btnResetAllEnglishToPersian.Click +=
            BtnResetAllEnglishToPersian_Click;

        Controls.Add(
            _btnResetAllEnglishToPersian);

        currentY +=
            ButtonHeight + 15;

        // =====================================================
        // SAVE
        // =====================================================

        _btnClose =
            CreateActionButton(
                "▣  " +
                Localization.Get("Save"),
                Color.LightBlue);

        _btnClose.Size =
            new Size(140, ButtonHeight);

        _btnClose.Location =
            new Point(
                FormWidth -
                Margin -
                _btnClose.Width,
                currentY);

        _btnClose.Click +=
            BtnClose_Click;

        Controls.Add(_btnClose);

        // =====================================================
        // FINAL FORM HEIGHT
        // =====================================================

        this.ClientSize =
            new Size(
                FormWidth,
                currentY +
                ButtonHeight +
                15);
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private Label CreateSectionTitle(string text)
    {
        return new Label
        {
            Text = text,

            Font = new Font(
                "Tahoma",
                10,
                FontStyle.Bold),

            Size =
                new Size(
                    FormWidth - (Margin * 2),
                    25),

            TextAlign =
                ContentAlignment.MiddleLeft,

            ForeColor =
                Color.FromArgb(20, 80, 150)
        };
    }

    private Label CreateInfoLabel(string text)
    {
        return new Label
        {
            Text = text,

            Font = new Font(
                "Tahoma",
                8.5f),

            Size =
                new Size(
                    FormWidth - (Margin * 2),
                    32),

            TextAlign =
                ContentAlignment.MiddleLeft,

            BackColor =
                Color.FromArgb(
                    240,
                    248,
                    255),

            ForeColor =
                Color.FromArgb(
                    25,
                    85,
                    150),

            BorderStyle =
                BorderStyle.FixedSingle,

            Padding =
                new Padding(8, 0, 8, 0)
        };
    }

    private Button CreateActionButton(
        string text,
        Color backColor)
    {
        var button = new Button
        {
            Text = text,

            Font =
                new Font(
                    "Tahoma",
                    9),

            FlatStyle =
                FlatStyle.Flat,

            BackColor =
                backColor,

            UseVisualStyleBackColor =
                false,

            TextAlign =
                ContentAlignment.MiddleCenter,

            Cursor =
                Cursors.Hand
        };

        button.FlatAppearance.BorderColor =
            Color.FromArgb(80, 80, 80);

        button.FlatAppearance.BorderSize = 1;

        return button;
    }

    // =========================================================
    // GRID CREATION
    // =========================================================

    private DataGridView CreateMappingGrid(
        bool isPersianToEnglish)
    {
        var grid = new DataGridView
        {
            AutoGenerateColumns = false,

            AllowUserToAddRows = false,

            AllowUserToDeleteRows = false,

            AllowUserToOrderColumns = false,

            AllowUserToResizeRows = false,

            ReadOnly = false,

            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,

            MultiSelect = false,

            ScrollBars =
                ScrollBars.Vertical,

            RowHeadersWidth = 30,

            RowHeadersVisible = true,

            RowTemplate =
            {
                Height = 30
            },

            BorderStyle =
                BorderStyle.FixedSingle,

            CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal,

            EnableHeadersVisualStyles = false,

            BackgroundColor =
                Color.White,

            GridColor =
                Color.LightGray,

            DefaultCellStyle =
            {
                Font =
                    new Font(
                        "Tahoma",
                        9),

                Alignment =
                    DataGridViewContentAlignment.MiddleCenter,

                SelectionBackColor =
                    Color.FromArgb(
                        190,
                        220,
                        235),

                SelectionForeColor =
                    Color.Black,

                BackColor =
                    Color.White,

                ForeColor =
                    Color.Black
            },

            ColumnHeadersDefaultCellStyle =
            {
                Font =
                    new Font(
                        "Tahoma",
                        9,
                        FontStyle.Bold),

                Alignment =
                    DataGridViewContentAlignment.MiddleCenter,

                BackColor =
                    Color.FromArgb(
                        230,
                        230,
                        230),

                ForeColor =
                    Color.Black,

                SelectionBackColor =
                    Color.FromArgb(
                        230,
                        230,
                        230),

                SelectionForeColor =
                    Color.Black
            }
        };

        // =====================================================
        // FIRST COLUMN - READ ONLY
        // =====================================================

        string firstHeader =
            isPersianToEnglish
                ? Localization.Get("GridColumnPersianCharReadOnly")
                : Localization.Get("GridColumnEnglishCharReadOnly");

        var firstColumn =
            new DataGridViewTextBoxColumn
            {
                HeaderText = firstHeader,

                ReadOnly = true,

                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,

                FillWeight = 34,

                MinimumWidth = 170,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable,

                DefaultCellStyle =
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,

                    BackColor =
                        Color.FromArgb(
                            245,
                            245,
                            245),

                    ForeColor =
                        Color.FromArgb(
                            70,
                            70,
                            70),

                    SelectionBackColor =
                        Color.FromArgb(
                            220,
                            230,
                            235)
                }
            };

        grid.Columns.Add(firstColumn);

        // =====================================================
        // SECOND COLUMN - EDITABLE
        // =====================================================

        string secondHeader =
            isPersianToEnglish
                ? Localization.Get("GridColumnEnglishCharEditable")
                : Localization.Get("GridColumnPersianCharEditable");

        var secondColumn =
            new DataGridViewTextBoxColumn
            {
                HeaderText = secondHeader,

                ReadOnly = false,

                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,

                FillWeight = 34,

                MinimumWidth = 170,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable,

                DefaultCellStyle =
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,

                    BackColor =
                        Color.White,

                    ForeColor =
                        Color.Black,

                    SelectionBackColor =
                        Color.FromArgb(
                            210,
                            235,
                            255),

                    Padding =
                        new Padding(
                            5,
                            2,
                            5,
                            2)
                }
            };

        grid.Columns.Add(secondColumn);

        // =====================================================
        // DELETE BUTTON
        // =====================================================

        var deleteColumn =
            new DataGridViewButtonColumn
            {
                HeaderText =
                    Localization.Get("Delete"),

                Text =
                    "🗑  " +
                    Localization.Get("Delete"),

                UseColumnTextForButtonValue = true,

                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,

                FillWeight = 16,

                MinimumWidth = 105,

                FlatStyle =
                    FlatStyle.Flat,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable,

                DefaultCellStyle =
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,

                    BackColor =
                        Color.MistyRose,

                    ForeColor =
                        Color.FromArgb(
                            150,
                            30,
                            30),

                    SelectionBackColor =
                        Color.FromArgb(
                            255,
                            205,
                            205),

                    SelectionForeColor =
                        Color.FromArgb(
                            120,
                            20,
                            20)
                }
            };

        deleteColumn.FlatStyle =
            FlatStyle.Flat;

        grid.Columns.Add(deleteColumn);

        // =====================================================
        // RESET BUTTON
        // =====================================================

        var resetColumn =
            new DataGridViewButtonColumn
            {
                HeaderText =
                    Localization.Get("Reset"),

                Text =
                    "↻  " +
                    Localization.Get("Reset"),

                UseColumnTextForButtonValue = true,

                AutoSizeMode =
                    DataGridViewAutoSizeColumnMode.Fill,

                FillWeight = 16,

                MinimumWidth = 105,

                FlatStyle =
                    FlatStyle.Flat,

                SortMode =
                    DataGridViewColumnSortMode.NotSortable,

                DefaultCellStyle =
                {
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter,

                    BackColor =
                        Color.LemonChiffon,

                    ForeColor =
                        Color.FromArgb(
                            100,
                            75,
                            0),

                    SelectionBackColor =
                        Color.FromArgb(
                            255,
                            240,
                            180),

                    SelectionForeColor =
                        Color.FromArgb(
                            90,
                            65,
                            0)
                }
            };

        resetColumn.FlatStyle =
            FlatStyle.Flat;

        grid.Columns.Add(resetColumn);

        // =====================================================
        // EVENTS
        // =====================================================

        if (isPersianToEnglish)
        {
            grid.CellContentClick +=
                DataGridViewPersianToEnglish_CellContentClick;

            grid.CellValidating +=
                DataGridViewPersianToEnglish_CellValidating;
        }
        else
        {
            grid.CellContentClick +=
                DataGridViewEnglishToPersian_CellContentClick;

            grid.CellValidating +=
                DataGridViewEnglishToPersian_CellValidating;
        }

        grid.CellParsing +=
            DataGridView_CellParsing;

        return grid;
    }

    // =========================================================
    // LOAD
    // =========================================================

    private void LoadMappings()
    {
        _dataGridViewPersianToEnglish!
            .Rows
            .Clear();

        _dataGridViewEnglishToPersian!
            .Rows
            .Clear();

        if (_settings.PersianToEnglishMap != null)
        {
            var sortedPersianChars =
                _settings
                    .PersianToEnglishMap
                    .Keys
                    .OrderBy(c => c)
                    .ToList();

            foreach (var persianChar in sortedPersianChars)
            {
                var englishChar =
                    _settings
                        .PersianToEnglishMap[persianChar];

                int rowIndex =
                    _dataGridViewPersianToEnglish
                        .Rows
                        .Add();

                var row =
                    _dataGridViewPersianToEnglish
                        .Rows[rowIndex];

                row.Cells[0].Value =
                    persianChar.ToString();

                row.Cells[1].Value =
                    englishChar.ToString();
            }
        }

        if (_settings.EnglishToPersianMap != null)
        {
            var sortedEnglishChars =
                _settings
                    .EnglishToPersianMap
                    .Keys
                    .OrderBy(c => c)
                    .ToList();

            foreach (var englishChar in sortedEnglishChars)
            {
                var persianChar =
                    _settings
                        .EnglishToPersianMap[englishChar];

                int rowIndex =
                    _dataGridViewEnglishToPersian
                        .Rows
                        .Add();

                var row =
                    _dataGridViewEnglishToPersian
                        .Rows[rowIndex];

                row.Cells[0].Value =
                    englishChar.ToString();

                row.Cells[1].Value =
                    persianChar.ToString();
            }
        }
    }

    // =========================================================
    // PERSIAN -> ENGLISH CLICK
    // =========================================================

    private void DataGridViewPersianToEnglish_CellContentClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
            return;

        var dataGridView =
            (DataGridView)sender!;

        // DELETE
        if (e.ColumnIndex == 2 &&
            dataGridView.Columns[e.ColumnIndex]
                is DataGridViewButtonColumn)
        {
            var persianCell =
                dataGridView
                    .Rows[e.RowIndex]
                    .Cells[0];

            var persianCharStr =
                persianCell
                    .Value?
                    .ToString();

            if (!string.IsNullOrEmpty(
                    persianCharStr) &&
                persianCharStr.Length == 1)
            {
                DeletePersianMapping(
                    persianCharStr[0]);
            }
        }

        // RESET
        else if (e.ColumnIndex == 3 &&
                 dataGridView.Columns[e.ColumnIndex]
                     is DataGridViewButtonColumn)
        {
            var persianCell =
                dataGridView
                    .Rows[e.RowIndex]
                    .Cells[0];

            var persianCharStr =
                persianCell
                    .Value?
                    .ToString();

            if (!string.IsNullOrEmpty(
                    persianCharStr) &&
                persianCharStr.Length == 1)
            {
                ResetPersianCharMapping(
                    persianCharStr[0]);
            }
        }
    }

    // =========================================================
    // ENGLISH -> PERSIAN CLICK
    // =========================================================

    private void DataGridViewEnglishToPersian_CellContentClick(
        object? sender,
        DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0)
            return;

        var dataGridView =
            (DataGridView)sender!;

        // DELETE
        if (e.ColumnIndex == 2 &&
            dataGridView.Columns[e.ColumnIndex]
                is DataGridViewButtonColumn)
        {
            var englishCell =
                dataGridView
                    .Rows[e.RowIndex]
                    .Cells[0];

            var englishCharStr =
                englishCell
                    .Value?
                    .ToString();

            if (!string.IsNullOrEmpty(
                    englishCharStr) &&
                englishCharStr.Length == 1)
            {
                DeleteEnglishMapping(
                    englishCharStr[0]);
            }
        }

        // RESET
        else if (e.ColumnIndex == 3 &&
                 dataGridView.Columns[e.ColumnIndex]
                     is DataGridViewButtonColumn)
        {
            var englishCell =
                dataGridView
                    .Rows[e.RowIndex]
                    .Cells[0];

            var englishCharStr =
                englishCell
                    .Value?
                    .ToString();

            if (!string.IsNullOrEmpty(
                    englishCharStr) &&
                englishCharStr.Length == 1)
            {
                ResetEnglishCharMapping(
                    englishCharStr[0]);
            }
        }
    }

    // =========================================================
    // CELL PARSING
    // =========================================================

    private void DataGridView_CellParsing(
        object? sender,
        DataGridViewCellParsingEventArgs e)
    {
        e.ParsingApplied = true;
    }

    // =========================================================
    // PERSIAN -> ENGLISH VALIDATION
    // =========================================================

    private void DataGridViewPersianToEnglish_CellValidating(
        object? sender,
        DataGridViewCellValidatingEventArgs e)
    {
        var gridView =
            (DataGridView)sender!;

        if (e.ColumnIndex < 0 ||
            e.RowIndex < 0)
            return;

        // فقط ستون دوم قابل ویرایش است
        if (e.ColumnIndex != 1)
            return;

        var persianCell =
            gridView
                .Rows[e.RowIndex]
                .Cells[0];

        var persianCharStr =
            persianCell
                .Value?
                .ToString();

        if (string.IsNullOrEmpty(
                persianCharStr) ||
            persianCharStr.Length != 1)
            return;

        string value =
            e.FormattedValue?
                .ToString() ??
            string.Empty;

        if (string.IsNullOrEmpty(value))
            return;

        if (value.Length > 1)
        {
            MessageBox.Show(
                Localization.Get("Error"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            e.Cancel = true;
            return;
        }

        char persianChar =
            persianCharStr[0];

        char englishChar =
            value[0];

        _settings
            .PersianToEnglishMap[persianChar] =
            englishChar;
    }

    // =========================================================
    // ENGLISH -> PERSIAN VALIDATION
    // =========================================================

    private void DataGridViewEnglishToPersian_CellValidating(
        object? sender,
        DataGridViewCellValidatingEventArgs e)
    {
        var gridView =
            (DataGridView)sender!;

        if (e.ColumnIndex < 0 ||
            e.RowIndex < 0)
            return;

        // فقط ستون دوم قابل ویرایش است
        if (e.ColumnIndex != 1)
            return;

        var englishCell =
            gridView
                .Rows[e.RowIndex]
                .Cells[0];

        var englishCharStr =
            englishCell
                .Value?
                .ToString();

        if (string.IsNullOrEmpty(
                englishCharStr) ||
            englishCharStr.Length != 1)
            return;

        string value =
            e.FormattedValue?
                .ToString() ??
            string.Empty;

        if (string.IsNullOrEmpty(value))
            return;

        if (value.Length > 1)
        {
            MessageBox.Show(
                Localization.Get("Error"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            e.Cancel = true;
            return;
        }

        char englishChar =
            englishCharStr[0];

        char persianChar =
            value[0];

        _settings
            .EnglishToPersianMap[englishChar] =
            persianChar;
    }

    // =========================================================
    // DELETE PERSIAN
    // =========================================================

    private void DeletePersianMapping(
        char persianChar)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "DeleteMappingConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        if (_settings
            .PersianToEnglishMap
            .ContainsKey(persianChar))
        {
            _settings
                .PersianToEnglishMap
                .Remove(persianChar);
        }

        LoadMappings();
    }

    // =========================================================
    // DELETE ENGLISH
    // =========================================================

    private void DeleteEnglishMapping(
        char englishChar)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "DeleteMappingConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        if (_settings
            .EnglishToPersianMap
            .ContainsKey(englishChar))
        {
            _settings
                .EnglishToPersianMap
                .Remove(englishChar);
        }

        LoadMappings();
    }

    // =========================================================
    // ADD PERSIAN -> ENGLISH
    // =========================================================

    private void AddPersianToEnglishMapping()
    {
        string? persianCharInput =
            InputBox.Show(
                Localization.Get(
                    "EnterPersianChar"),

                Localization.Get(
                    "AddMapping"),

                "");

        if (string.IsNullOrEmpty(
                persianCharInput) ||
            persianCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get(
                    "InvalidPersianChar"),

                Localization.Get(
                    "Error"),

                MessageBoxButtons.OK,

                MessageBoxIcon.Error);

            return;
        }

        char persianChar =
            persianCharInput[0];

        string? englishCharInput =
            InputBox.Show(
                Localization.Get(
                    "EnterEnglishChar"),

                Localization.Get(
                    "AddMapping"),

                "");

        if (string.IsNullOrEmpty(
                englishCharInput) ||
            englishCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get(
                    "InvalidEnglishChar"),

                Localization.Get(
                    "Error"),

                MessageBoxButtons.OK,

                MessageBoxIcon.Error);

            return;
        }

        char englishChar =
            englishCharInput[0];

        _settings
            .PersianToEnglishMap[persianChar] =
            englishChar;

        LoadMappings();

        MessageBox.Show(
            Localization.Get(
                "MappingAddedSuccess"),

            Localization.Get(
                "Success"),

            MessageBoxButtons.OK,

            MessageBoxIcon.Information);
    }

    // =========================================================
    // ADD ENGLISH -> PERSIAN
    // =========================================================

    private void AddEnglishToPersianMapping()
    {
        string? englishCharInput =
            InputBox.Show(
                Localization.Get(
                    "EnterEnglishChar"),

                Localization.Get(
                    "AddMapping"),

                "");

        if (string.IsNullOrEmpty(
                englishCharInput) ||
            englishCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get(
                    "InvalidEnglishChar"),

                Localization.Get(
                    "Error"),

                MessageBoxButtons.OK,

                MessageBoxIcon.Error);

            return;
        }

        char englishChar =
            englishCharInput[0];

        string? persianCharInput =
            InputBox.Show(
                Localization.Get(
                    "EnterPersianChar"),

                Localization.Get(
                    "AddMapping"),

                "");

        if (string.IsNullOrEmpty(persianCharInput) || persianCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get(
                    "InvalidPersianChar"),

                Localization.Get(
                    "Error"),

                MessageBoxButtons.OK,

                MessageBoxIcon.Error);

            return;
        }

        char persianChar =
            persianCharInput[0];

        _settings
            .EnglishToPersianMap[englishChar] =
            persianChar;

        LoadMappings();

        MessageBox.Show(
            Localization.Get(
                "MappingAddedSuccess"),

            Localization.Get(
                "Success"),

            MessageBoxButtons.OK,

            MessageBoxIcon.Information);
    }

    // =========================================================
    // RESET PERSIAN CHARACTER
    // =========================================================

    private void ResetPersianCharMapping(
        char persianChar)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "ResetKeyConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        if (_settings
            .PersianToEnglishMap
            .ContainsKey(persianChar))
        {
            _settings
                .PersianToEnglishMap
                .Remove(persianChar);
        }

        LoadMappings();
    }

    // =========================================================
    // RESET ENGLISH CHARACTER
    // =========================================================

    private void ResetEnglishCharMapping(
        char englishChar)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "ResetKeyConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        if (_settings
            .EnglishToPersianMap
            .ContainsKey(englishChar))
        {
            _settings
                .EnglishToPersianMap
                .Remove(englishChar);
        }

        LoadMappings();
    }

    // =========================================================
    // BUTTON EVENTS
    // =========================================================

    private void BtnAddPersianToEnglish_Click(
        object? sender,
        EventArgs e)
    {
        AddPersianToEnglishMapping();
    }

    private void BtnAddEnglishToPersian_Click(
        object? sender,
        EventArgs e)
    {
        AddEnglishToPersianMapping();
    }

    // =========================================================
    // RESET ALL - PERSIAN -> ENGLISH
    // =========================================================

    private void BtnResetAllPersianToEnglish_Click(
        object? sender,
        EventArgs e)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "ResetAllConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        _settings
            .PersianToEnglishMap
            .Clear();

        LoadMappings();
    }

    // =========================================================
    // RESET ALL - ENGLISH -> PERSIAN
    // =========================================================

    private void BtnResetAllEnglishToPersian_Click(
        object? sender,
        EventArgs e)
    {
        var result =
            MessageBox.Show(
                Localization.Format(
                    "ResetAllConfirm"),

                Localization.Get(
                    "Attention"),

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
            return;

        _settings
            .EnglishToPersianMap
            .Clear();

        LoadMappings();
    }

    // =========================================================
    // SAVE
    // =========================================================

    private void BtnClose_Click(
        object? sender,
        EventArgs e)
    {
        SettingsManager.Save(_settings);

        DialogResult =
            DialogResult.OK;

        Close();
    }
}


// =============================================================
// INPUT BOX
// =============================================================

public static class InputBox
{
    public static string? Show(
        string prompt,
        string title,
        string defaultValue)
    {
        using var form =
            new Form();

        form.Text = title;

        form.FormBorderStyle =
            FormBorderStyle.FixedDialog;

        form.MaximizeBox = false;
        form.MinimizeBox = false;

        form.StartPosition =
            FormStartPosition.CenterParent;

        form.Size =
            new Size(350, 170);

        form.Font =
            new Font("Tahoma", 9);

        var label =
            new Label
            {
                Text = prompt,

                AutoSize = false,

                Location =
                    new Point(20, 15),

                Size =
                    new Size(300, 30),

                TextAlign =
                    ContentAlignment.MiddleLeft
            };

        form.Controls.Add(label);

        var textBox =
            new TextBox
            {
                Text = defaultValue,

                Location =
                    new Point(20, 48),

                Size =
                    new Size(300, 25),

                MaxLength = 1,

                TextAlign =
                    HorizontalAlignment.Center,

                Font =
                    new Font(
                        "Tahoma",
                        11)
            };

        form.Controls.Add(textBox);

        var okButton =
            new Button
            {
                Text =
                    Localization.Get("OK"),

                DialogResult =
                    DialogResult.OK,

                Location =
                    new Point(110, 88),

                Size =
                    new Size(90, 30),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.LightBlue
            };

        form.Controls.Add(okButton);

        var cancelButton =
            new Button
            {
                Text =
                    Localization.Get("Cancel"),

                DialogResult =
                    DialogResult.Cancel,

                Location =
                    new Point(205, 88),

                Size =
                    new Size(90, 30),

                FlatStyle =
                    FlatStyle.Flat,

                BackColor =
                    Color.LightGray
            };

        form.Controls.Add(cancelButton);

        form.AcceptButton =
            okButton;

        form.CancelButton =
            cancelButton;

        form.Shown +=
            (_, _) =>
            {
                textBox.Focus();
                textBox.SelectAll();
            };

        var result =
            form.ShowDialog();

        return result ==
               DialogResult.OK
            ? textBox.Text
            : null;
    }
}