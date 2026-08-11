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
    private Button? _btnAddPersianToEnglish;
    private Button? _btnAddEnglishToPersian;
    private Button? _btnResetAllPersianToEnglish;
    private Button? _btnResetAllEnglishToPersian;
    private Button? _btnClose;
    private Label? _titleLabel;
    private Label? _labelPersianToEnglish;
    private Label? _labelEnglishToPersian;
    private const int RowHeight = 30;
    private const int DefaultGridHeight = 140;
    private const int MaxGridHeight = 250;

    public KeyboardMappingsForm(AppSettings settings)
    {
        _settings = settings ?? new AppSettings();
        if (_settings.PersianToEnglishMap == null)
            _settings.PersianToEnglishMap = new Dictionary<char, char>();
        if (_settings.EnglishToPersianMap == null)
            _settings.EnglishToPersianMap = new Dictionary<char, char>();

        Localization.SetLanguage(_settings.Language);

        InitializeControls();
        LoadMappings();
    }

    private void InitializeControls()
    {
        this.Text = Localization.Get("KeyboardMappingsTitle");
        
        // محاسبه ارتفاع داینامیک فرم
        int formHeight = CalculateFormHeight();
        this.Size = new System.Drawing.Size(600, formHeight);
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
            Text = Localization.Get("PersianToEnglish"),
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
            Size = new System.Drawing.Size(formWidth - (2 * marginX), DefaultGridHeight),
            AutoGenerateColumns = false,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AllowUserToOrderColumns = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Dock = DockStyle.None,
             ScrollBars = ScrollBars.Both
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

        // Delete button column
        var deleteColumn = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Delete"),
            UseColumnTextForButtonValue = true,
            Width = 70
        };
        _dataGridViewPersianToEnglish.Columns.Add(deleteColumn);

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
        currentY += DefaultGridHeight + 15;

        // Add button for Persian to English
        _btnAddPersianToEnglish = new Button
        {
            Text = "➕ " + Localization.Get("AddMapping"),
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.LightGreen
        };
        _btnAddPersianToEnglish.Click += BtnAddPersianToEnglish_Click;
        this.Controls.Add(_btnAddPersianToEnglish);
        currentY += 45;

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
        currentY += 45;

        // English to Persian section
        _labelEnglishToPersian = new Label
        {
            Text = Localization.Get("EnglishToPersian"),
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
            Size = new System.Drawing.Size(formWidth - (2 * marginX), DefaultGridHeight),
            AutoGenerateColumns = false,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AllowUserToOrderColumns = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Dock = DockStyle.None,
            ScrollBars = ScrollBars.Both
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

        // Delete button column
        var deleteColumn2 = new DataGridViewButtonColumn
        {
            HeaderText = Localization.Get("Delete"),
            UseColumnTextForButtonValue = true,
            Width = 70
        };
        _dataGridViewEnglishToPersian.Columns.Add(deleteColumn2);

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
        currentY += DefaultGridHeight + 15;

        // Add button for English to Persian
        _btnAddEnglishToPersian = new Button
        {
            Text = "➕ " + Localization.Get("AddMapping"),
            Location = new System.Drawing.Point(marginX, currentY),
            Size = new System.Drawing.Size(120, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.LightGreen
        };
        _btnAddEnglishToPersian.Click += BtnAddEnglishToPersian_Click;
        this.Controls.Add(_btnAddEnglishToPersian);
        currentY += 45;

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
        currentY += 45;

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
        
        // تنظیم ارتفاع نهایی فرم بر اساس محتوا
        this.ClientSize = new System.Drawing.Size(formWidth, currentY + 30);
    }

    private int CalculateFormHeight()
    {
        int currentY = 20 + 45 + 30 + DefaultGridHeight + 15 + 45 + 45 + 30 + DefaultGridHeight + 15 + 45 + 45 + 32 + 30;
        return currentY;
    }

    private void LoadMappings()
    {
        _dataGridViewPersianToEnglish!.Rows.Clear();
        _dataGridViewEnglishToPersian!.Rows.Clear();

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
            var dataGridView = (DataGridView)sender!;
            
            // ستون حذف (delete) - اولین دکمه
            if (e.ColumnIndex == 2 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var persianCell = dataGridView.Rows[e.RowIndex].Cells[0];
                var persianCharStr = persianCell.Value?.ToString();
                if (!string.IsNullOrEmpty(persianCharStr) && persianCharStr.Length == 1)
                {
                    DeletePersianMapping(persianCharStr[0]);
                }
            }
            // ستون برگردان (reset) - دومین دکمه
            else if (e.ColumnIndex == 3 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var persianCell = dataGridView.Rows[e.RowIndex].Cells[0];
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
            var dataGridView = (DataGridView)sender!;
            
            // ستون حذف (delete) - اولین دکمه
            if (e.ColumnIndex == 2 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var englishCell = dataGridView.Rows[e.RowIndex].Cells[0];
                var englishCharStr = englishCell.Value?.ToString();
                if (!string.IsNullOrEmpty(englishCharStr) && englishCharStr.Length == 1)
                {
                    DeleteEnglishMapping(englishCharStr[0]);
                }
            }
            // ستون برگردان (reset) - دومین دکمه
            else if (e.ColumnIndex == 3 && dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var englishCell = dataGridView.Rows[e.RowIndex].Cells[0];
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

    private void DeletePersianMapping(char persianChar)
    {
        var result = MessageBox.Show(
            Localization.Format("DeleteMappingConfirm"),
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

    private void DeleteEnglishMapping(char englishChar)
    {
        var result = MessageBox.Show(
            Localization.Format("DeleteMappingConfirm"),
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

    private void AddPersianToEnglishMapping()
    {
        string? persianCharInput = InputBox.Show(
            Localization.Get("EnterPersianChar"),
            Localization.Get("AddMapping"),
            ""
        );

        if (string.IsNullOrEmpty(persianCharInput) || persianCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get("InvalidPersianChar"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        char persianChar = persianCharInput[0];

        string? englishCharInput = InputBox.Show(
            Localization.Get("EnterEnglishChar"),
            Localization.Get("AddMapping"),
            ""
        );

        if (string.IsNullOrEmpty(englishCharInput) || englishCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get("InvalidEnglishChar"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        char englishChar = englishCharInput[0];

        if (_settings.PersianToEnglishMap.ContainsKey(persianChar))
        {
            _settings.PersianToEnglishMap[persianChar] = englishChar;
        }
        else
        {
            _settings.PersianToEnglishMap.Add(persianChar, englishChar);
        }

        LoadMappings();
        MessageBox.Show(
            Localization.Get("MappingAddedSuccess"),
            Localization.Get("Success"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private void AddEnglishToPersianMapping()
    {
        string? englishCharInput = InputBox.Show(
            Localization.Get("EnterEnglishChar"),
            Localization.Get("AddMapping"),
            ""
        );

        if (string.IsNullOrEmpty(englishCharInput) || englishCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get("InvalidEnglishChar"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        char englishChar = englishCharInput[0];

        string? persianCharInput = InputBox.Show(
            Localization.Get("EnterPersianChar"),
            Localization.Get("AddMapping"),
            ""
        );

        if (string.IsNullOrEmpty(persianCharInput) || persianCharInput.Length != 1)
        {
            MessageBox.Show(
                Localization.Get("InvalidPersianChar"),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        char persianChar = persianCharInput[0];

        if (_settings.EnglishToPersianMap.ContainsKey(englishChar))
        {
            _settings.EnglishToPersianMap[englishChar] = persianChar;
        }
        else
        {
            _settings.EnglishToPersianMap.Add(englishChar, persianChar);
        }

        LoadMappings();
        MessageBox.Show(
            Localization.Get("MappingAddedSuccess"),
            Localization.Get("Success"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
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

    private void BtnAddPersianToEnglish_Click(object? sender, EventArgs e)
    {
        AddPersianToEnglishMapping();
    }

    private void BtnAddEnglishToPersian_Click(object? sender, EventArgs e)
    {
        AddEnglishToPersianMapping();
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

/// <summary>
/// Simple input box for entering single characters
/// </summary>
public static class InputBox
{
    public static string? Show(string prompt, string title, string defaultValue)
    {
        using var form = new Form();
        form.Text = title;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Size = new System.Drawing.Size(350, 150);
        form.Font = new System.Drawing.Font("Tahoma", 9);

        var label = new Label
        {
            Text = prompt,
            AutoSize = true,
            Location = new System.Drawing.Point(20, 20),
            Dock = DockStyle.None,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        form.Controls.Add(label);

        var textBox = new TextBox
        {
            Text = defaultValue,
            Location = new System.Drawing.Point(20, 50),
            Size = new System.Drawing.Size(300, 25)
        };
        form.Controls.Add(textBox);

        var okButton = new Button
        {
            Text = Localization.Get("OK"),
            DialogResult = DialogResult.OK,
            Location = new System.Drawing.Point(110, 90),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.LightBlue
        };
        form.Controls.Add(okButton);

        var cancelButton = new Button
        {
            Text = Localization.Get("Cancel"),
            DialogResult = DialogResult.Cancel,
            Location = new System.Drawing.Point(200, 90),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.LightGray
        };
        form.Controls.Add(cancelButton);

        form.AcceptButton = okButton;

        var result = form.ShowDialog();
        return result == DialogResult.OK ? textBox.Text : null;
    }
}