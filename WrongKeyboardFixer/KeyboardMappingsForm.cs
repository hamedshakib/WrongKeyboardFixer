using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public partial class KeyboardMappingsForm : Form
{
    private readonly AppSettings _settings;
    private DataGridView? _dataGridViewPersianToEnglish;
    private DataGridView? _dataGridViewEnglishToPersian;
    private Button? _btnResetAllPersianToEnglish;
    private Button? _btnResetAllEnglishToPersian;
    private Button? _btnClose;
    private Label? _titleLabel;
    private Label? _labelPersianToEnglish;
    private Label? _labelEnglishToPersian;

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();
        // Ensure PersianToEnglishMap and EnglishToPersianMap are initialized
        if (_settings.PersianToEnglishMap == null)
            _settings.PersianToEnglishMap = new Dictionary<char, char>();
        if (_settings.EnglishToPersianMap == null)
            _settings.EnglishToPersianMap = new Dictionary<char, char>();

        // Set language before creating controls
        Localization.SetLanguage(_settings.Language);

        InitializeControls();
        LoadMappings();
    }

    private void InitializeControls()
    {
        this.Text = Localization.Get("KeyboardMappingsTitle");
        this.Size = new System.Drawing.Size(600, 620);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Tahoma", 9);
        this.Icon = IconLoader.GetIcon();
        this.RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        this.RightToLeftLayout = Localization.IsRtl;

        int marginX = 20;
        int currentY = 20;
        int formWidth = this.ClientSize.Width;

        // Title label
        _titleLabel = new Label
        {
            Text = Localization.Get("KeyboardMappingsTitle"),
            Font = new System.Drawing.Font("Tahoma", 11, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(formWidth - (2 * marginX), 30),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        this.Controls.Add(_titleLabel);
        currentY += 45;

        // Persian to English section
        _labelPersianToEnglish = new Label
        {
            Text = "فارسی → انگلیسی",
            Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(formWidth - (2 * marginX), 25)
        };
        this.Controls.Add(_labelPersianToEnglish);
        currentY += 30;

        // Persian to English data grid view
        _dataGridViewPersianToEnglish = new DataGridView
        {
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(formWidth - (2 * marginX), 140),
            AutoGenerateColumns = false,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AllowUserToOrderColumns = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Dock = DockStyle.None
        };

        // Persian character column (read only)
        var persianColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = Localization.Get("PersianChar"),
            ReadOnly = true,
            Width = 100
        };
        _dataGridViewPersianToEnglish.Columns.Add(persianColumn);

        // English character column (editable)
        var englishColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = Localization.Get("EnglishChar"),
            Width = 100
        };
        _dataGridViewPersianToEnglish.Columns.Add(englishColumn);

        // Reset button column
        var resetColumn = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Reset"),
            UseColumnTextForButtonValue = true,
            Width = 80
        };
        _dataGridViewPersianToEnglish.Columns.Add(resetColumn);

        _dataGridViewPersianToEnglish.DefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 9);
        _dataGridViewPersianToEnglish.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 9);
        _dataGridViewPersianToEnglish.EnableHeadersVisualStyles = false;
        _dataGridViewPersianToEnglish.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
        _dataGridViewPersianToEnglish.CellContentClick += DataGridViewPersianToEnglish_CellContentClick;
        _dataGridViewPersianToEnglish.CellParsing += DataGridView_CellParsing;
        _dataGridViewPersianToEnglish.CellValidating += DataGridViewPersianToEnglish_CellValidating;

        this.Controls.Add(_dataGridViewPersianToEnglish);
        currentY += 160;

        // Reset All button for Persian to English
        _btnResetAllPersianToEnglish = new Button
        {
            Text = Localization.Get("ResetAll"),
            Location = new System.Drawing.Point(formWidth - marginX - 130, currentY),
            Size = new System.Drawing.Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.Orange
        };
        _btnResetAllPersianToEnglish.Click += BtnResetAllPersianToEnglish_Click;
        this.Controls.Add(_btnResetAllPersianToEnglish);
        currentY += 50;

        // English to Persian section
        _labelEnglishToPersian = new Label
        {
            Text = "انگلیسی → فارسی",
            Font = new System.Drawing.Font("Tahoma", 10, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(formWidth - (2 * marginX), 25)
        };
        this.Controls.Add(_labelEnglishToPersian);
        currentY += 30;

        // English to Persian data grid view
        _dataGridViewEnglishToPersian = new DataGridView
        {
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(formWidth - (2 * marginX), 140),
            AutoGenerateColumns = false,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AllowUserToOrderColumns = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Dock = DockStyle.None
        };

        // English character column (read only)
        var englishColumn2 = new DataGridViewTextBoxColumn
        {
            HeaderText = Localization.Get("EnglishChar"),
            ReadOnly = true,
            Width = 100
        };
        _dataGridViewEnglishToPersian.Columns.Add(englishColumn2);

        // Persian character column (editable)
        var persianColumn2 = new DataGridViewTextBoxColumn
        {
            HeaderText = Localization.Get("PersianChar"),
            Width = 100
        };
        _dataGridViewEnglishToPersian.Columns.Add(persianColumn2);

        // Reset button column
        var resetColumn2 = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Reset"),
            UseColumnTextForButtonValue = true,
            Width = 80
        };
        _dataGridViewEnglishToPersian.Columns.Add(resetColumn2);

        _dataGridViewEnglishToPersian.DefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 9);
        _dataGridViewEnglishToPersian.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 9);
        _dataGridViewEnglishToPersian.EnableHeadersVisualStyles = false;
        _dataGridViewEnglishToPersian.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
        _dataGridViewEnglishToPersian.CellContentClick += DataGridViewEnglishToPersian_CellContentClick;
        _dataGridViewEnglishToPersian.CellParsing += DataGridView_CellParsing;
        _dataGridViewEnglishToPersian.CellValidating += DataGridViewEnglishToPersian_CellValidating;

        this.Controls.Add(_dataGridViewEnglishToPersian);
        currentY += 160;

        // Reset All button for English to Persian
        _btnResetAllEnglishToPersian = new Button
        {
            Text = Localization.Get("ResetAll"),
            Location = new System.Drawing.Point(formWidth - marginX - 130, currentY),
            Size = new System.Drawing.Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.Orange
        };
        _btnResetAllEnglishToPersian.Click += BtnResetAllEnglishToPersian_Click;
        this.Controls.Add(_btnResetAllEnglishToPersian);
        currentY += 50;

        // Close/Save button
        _btnClose = new Button
        {
            Text = Localization.Get("Save"),
            Location = new System.Drawing.Point(formWidth - marginX - 130, currentY),
            Size = new System.Drawing.Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.LightBlue
        };
        _btnClose.Click += BtnClose_Click;
        this.Controls.Add(_btnClose);
    }

    private void LoadMappings()
    {
        _dataGridViewPersianToEnglish!.Rows.Clear();
        _dataGridViewEnglishToPersian!.Rows.Clear();

        // Load Persian to English mappings
        if (_settings.PersianToEnglishMap != null)
        {
            var sortedPersianChars = _settings.PersianToEnglishMap.Keys.OrderBy(c => c).ToList();
            foreach (var persianChar in sortedPersianChars)
            {
                var englishChar = _settings.PersianToEnglishMap[persianChar];
                int rowIndex = _dataGridViewPersianToEnglish.Rows.Add();
                var row = _dataGridViewPersianToEnglish.Rows[rowIndex];
                row.Cells[0].Value = persianChar.ToString();
                row.Cells[1].Value = englishChar.ToString();
            }
        }

        // Load English to Persian mappings
        if (_settings.EnglishToPersianMap != null)
        {
            var sortedEnglishChars = _settings.EnglishToPersianMap.Keys.OrderBy(c => c).ToList();
            foreach (var englishChar in sortedEnglishChars)
            {
                var persianChar = _settings.EnglishToPersianMap[englishChar];
                int rowIndex = _dataGridViewEnglishToPersian.Rows.Add();
                var row = _dataGridViewEnglishToPersian.Rows[rowIndex];
                row.Cells[0].Value = englishChar.ToString();
                row.Cells[1].Value = persianChar.ToString();
            }
        }
    }

    private void DataGridViewPersianToEnglish_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            if (_dataGridViewPersianToEnglish!.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.ColumnIndex == _dataGridViewPersianToEnglish.Columns.Count - 1)
            {
                var persianCell = _dataGridViewPersianToEnglish.Rows[e.RowIndex].Cells[0];
                var persianCharStr = persianCell.Value?.ToString();
                if (!string.IsNullOrEmpty(persianCharStr) && persianCharStr.Length == 1)
                {
                    ResetPersianCharMapping(persianCharStr[0]);
                }
            }
        }
    }

    private void DataGridViewEnglishToPersian_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            if (_dataGridViewEnglishToPersian!.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.ColumnIndex == _dataGridViewEnglishToPersian.Columns.Count - 1)
            {
                var englishCell = _dataGridViewEnglishToPersian.Rows[e.RowIndex].Cells[0];
                var englishCharStr = englishCell.Value?.ToString();
                if (!string.IsNullOrEmpty(englishCharStr) && englishCharStr.Length == 1)
                {
                    ResetEnglishCharMapping(englishCharStr[0]);
                }
            }
        }
    }

    private void DataGridView_CellParsing(object? sender, DataGridViewCellParsingEventArgs e)
    {
        e.ParsingApplied = true;
    }

    private void DataGridViewPersianToEnglish_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        var gridView = (DataGridView)sender!;
        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            var persianCell = gridView.Rows[e.RowIndex].Cells[0];
            var persianCharStr = persianCell.Value?.ToString();

            if (string.IsNullOrEmpty(persianCharStr) || persianCharStr.Length != 1)
                return;

            char persianChar = persianCharStr[0];

            if (string.IsNullOrEmpty(e.FormattedValue?.ToString() ?? ""))
                return;

            string value = (e.FormattedValue?.ToString() ?? "")!;
            if (value.Length > 1)
            {
                MessageBox.Show(Localization.Get("Error"), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
            else
            {
                char englishChar = value[0];
                if (_settings.PersianToEnglishMap.ContainsKey(persianChar))
                {
                    _settings.PersianToEnglishMap[persianChar] = englishChar;
                }
                else
                {
                    _settings.PersianToEnglishMap[persianChar] = englishChar;
                }
            }
        }
    }

    private void DataGridViewEnglishToPersian_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        var gridView = (DataGridView)sender!;
        if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
        {
            var englishCell = gridView.Rows[e.RowIndex].Cells[0];
            var englishCharStr = englishCell.Value?.ToString();

            if (string.IsNullOrEmpty(englishCharStr) || englishCharStr.Length != 1)
                return;

            char englishChar = englishCharStr[0];

            if (string.IsNullOrEmpty(e.FormattedValue?.ToString() ?? ""))
                return;

            string value = (e.FormattedValue?.ToString() ?? "")!;
            if (value.Length > 1)
            {
                MessageBox.Show(Localization.Get("Error"), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
            else
            {
                char persianChar = value[0];
                if (_settings.EnglishToPersianMap.ContainsKey(englishChar))
                {
                    _settings.EnglishToPersianMap[englishChar] = persianChar;
                }
                else
                {
                    _settings.EnglishToPersianMap[englishChar] = persianChar;
                }
            }
        }
    }

    private void ResetPersianCharMapping(char persianChar)
    {
        var result = MessageBox.Show(
            Localization.Format("ResetKeyConfirm"),
            Localization.Get("Attention"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            if (_settings.PersianToEnglishMap.ContainsKey(persianChar))
            {
                _settings.PersianToEnglishMap.Remove(persianChar);
            }
            LoadMappings();
        }
    }

    private void ResetEnglishCharMapping(char englishChar)
    {
        var result = MessageBox.Show(
            Localization.Format("ResetKeyConfirm"),
            Localization.Get("Attention"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            if (_settings.EnglishToPersianMap.ContainsKey(englishChar))
            {
                _settings.EnglishToPersianMap.Remove(englishChar);
            }
            LoadMappings();
        }
    }

    private void BtnResetAllPersianToEnglish_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            Localization.Format("ResetAllConfirm"),
            Localization.Get("Attention"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _settings.PersianToEnglishMap.Clear();
            LoadMappings();
        }
    }

    private void BtnResetAllEnglishToPersian_Click(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            Localization.Format("ResetAllConfirm"),
            Localization.Get("Attention"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _settings.EnglishToPersianMap.Clear();
            LoadMappings();
        }
    }

    private void BtnClose_Click(object? sender, EventArgs e)
    {
        SettingsManager.Save(_settings);
        DialogResult = DialogResult.OK;
        Close();
    }
}